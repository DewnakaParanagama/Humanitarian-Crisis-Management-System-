using System.Text;
using System.Text.Json;
using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.AI.DTOs;
using Humanitarian.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Humanitarian.Infrastructure.Services;

public class GeminiAIAnalysisService : IAIAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IAppDbContext _context;
    private readonly string _apiKey;

    public GeminiAIAnalysisService(
        HttpClient httpClient,
        IAppDbContext context,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API key is missing.");
    }

    public async Task<AIAnalysisResponse> AnalyzeRequestAsync(
        AnalyzeRequestDto request,
        CancellationToken cancellationToken)
    {
        var language = SupportedLanguages.Normalize(request.Language);
        if (!SupportedLanguages.IsSupported(language))
        {
            throw new ArgumentException(
                "Language must be English, Sinhala, Tamil, en, si, or ta.",
                nameof(request));
        }

        var shelters = await _context.Shelters
            .AsNoTracking()
            .Where(shelter => shelter.Status == ShelterStatus.Open &&
                shelter.CurrentOccupancy < shelter.TotalCapacity)
            .Select(shelter => new
            {
                shelter.Name,
                shelter.Location,
                shelter.District,
                shelter.Latitude,
                shelter.Longitude,
                AvailableCapacity = shelter.TotalCapacity - shelter.CurrentOccupancy
            })
            .ToListAsync(cancellationToken);

        var resources = await _context.Resources
            .AsNoTracking()
            .Where(resource => resource.Quantity > 0 &&
                resource.Status == "Available")
            .OrderByDescending(resource => resource.Quantity)
            .Take(25)
            .Select(resource => new
            {
                resource.ItemName,
                resource.Category,
                resource.Quantity,
                resource.Unit,
                resource.Location
            })
            .ToListAsync(cancellationToken);

        var shelterContext = shelters
            .OrderBy(shelter => DistanceInKilometers(
                request.Latitude,
                request.Longitude,
                shelter.Latitude,
                shelter.Longitude))
            .Take(10)
            .Select(shelter =>
                $"{shelter.Name} | {shelter.Location} | {shelter.District} | " +
                $"available capacity: {shelter.AvailableCapacity} | " +
                $"coordinates: {shelter.Latitude?.ToString() ?? "unknown"}," +
                $"{shelter.Longitude?.ToString() ?? "unknown"}")
            .ToList();

        var resourceContext = resources
            .Select(resource =>
                $"{resource.ItemName} ({resource.Category}) | " +
                $"quantity: {resource.Quantity} {resource.Unit} | " +
                $"location: {resource.Location}")
            .ToList();

        var prompt = $"""
            You are an AI assistant supporting humanitarian emergency coordination.
            Analyze the citizen assistance request and produce a concise summary in
            {language}. Keep the summary and recommendation clear for a coordinator;
            use English when the requested language is English.

            Request:
            Category: {request.Category}
            Reported urgency: {request.Urgency}
            Description: {request.Description}
            Location: {request.Location}
            Coordinates: {request.Latitude?.ToString() ?? "unknown"}, {request.Longitude?.ToString() ?? "unknown"}
            Affected people: {request.Affected}

            Live open shelters with capacity:
            {(shelterContext.Count == 0 ? "None currently available." : string.Join("\n", shelterContext))}

            Live available resources:
            {(resourceContext.Count == 0 ? "None currently available." : string.Join("\n", resourceContext))}

            Instructions:
            1. Assess urgency based on the request and live context.
            2. Return confidenceScore as an integer from 0 to 100.
            3. Return urgencyScore as a number from 1 to 10.
            4. Recommend a suitable humanitarian action using only the live data above.
            5. Identify the nearest relevant live resource or shelter when possible.
            6. Never invent names, quantities, locations, distances, or supplies.
            7. AI recommendations are advisory; a human coordinator makes final decisions.
            """;

        var schema = new
        {
            type = "OBJECT",
            properties = new
            {
                summary = new { type = "STRING" },
                confidenceScore = new { type = "INTEGER" },
                calculatedUrgency = new { type = "STRING" },
                urgencyScore = new { type = "NUMBER" },
                recommendedAction = new { type = "STRING" },
                nearestResource = new { type = "STRING" }
            },
            required = new[]
            {
                "summary", "confidenceScore", "calculatedUrgency",
                "urgencyScore", "recommendedAction", "nearestResource"
            }
        };

        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseJsonSchema = schema
            }
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent");
        httpRequest.Headers.Add("x-goog-api-key", _apiKey);
        httpRequest.Content = content;

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine($"Gemini Status: {(int)response.StatusCode}");
            Console.WriteLine($"Gemini Error: {responseBody}");

            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(responseBody);
            var aiText = document.RootElement.GetProperty("candidates")[0]
                .GetProperty("content").GetProperty("parts")[0]
                .GetProperty("text").GetString();

            if (string.IsNullOrWhiteSpace(aiText))
            {
                throw new InvalidOperationException("Gemini returned an empty response.");
            }

            var result = JsonSerializer.Deserialize<AIAnalysisResponse>(
                aiText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || string.IsNullOrWhiteSpace(result.Summary) ||
                result.ConfidenceScore is < 0 or > 100 ||
                result.UrgencyScore is < 1 or > 10 ||
                string.IsNullOrWhiteSpace(result.CalculatedUrgency) ||
                string.IsNullOrWhiteSpace(result.RecommendedAction) ||
                string.IsNullOrWhiteSpace(result.NearestResource))
            {
                throw new InvalidOperationException("Gemini returned an invalid analysis.");
            }

            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return CreateFallbackAnalysis(language);
        }
        catch (HttpRequestException)
        {
            return CreateFallbackAnalysis(language);
        }
        catch (JsonException)
        {
            return CreateFallbackAnalysis(language);
        }
        catch (InvalidOperationException)
        {
            return CreateFallbackAnalysis(language);
        }
    }

    private static AIAnalysisResponse CreateFallbackAnalysis(string language) => new()
    {
        Summary = language == SupportedLanguages.English
            ? "AI analysis is currently unavailable."
            : "AI analysis is currently unavailable; please review this request manually.",
        ConfidenceScore = 0,
        CalculatedUrgency = "Unknown",
        UrgencyScore = 1,
        RecommendedAction = "Please review this request manually.",
        NearestResource = "None available."
    };

    private static double DistanceInKilometers(
        double? latitude1,
        double? longitude1,
        double? latitude2,
        double? longitude2)
    {
        if (!latitude1.HasValue || !longitude1.HasValue ||
            !latitude2.HasValue || !longitude2.HasValue)
        {
            return double.MaxValue;
        }

        const double earthRadius = 6371;
        var latitudeDelta = DegreesToRadians(latitude2.Value - latitude1.Value);
        var longitudeDelta = DegreesToRadians(longitude2.Value - longitude1.Value);
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
            Math.Cos(DegreesToRadians(latitude1.Value)) *
            Math.Cos(DegreesToRadians(latitude2.Value)) *
            Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);

        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}