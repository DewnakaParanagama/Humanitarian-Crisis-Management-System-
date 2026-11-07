using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.AI.DTOs;
using Microsoft.Extensions.Configuration;

namespace Humanitarian.Infrastructure.Services;

public class GeminiAIAnalysisService : IAIAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiAIAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException(
                "Gemini API key is missing.");
    }

    public async Task<AIAnalysisResponse> AnalyzeRequestAsync(
        AnalyzeRequestDto request,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            You are an AI assistant supporting humanitarian emergency coordination.

            Analyze this citizen assistance request.

            Category: {request.Category}
            Reported urgency: {request.Urgency}
            Description: {request.Description}
            Location: {request.Location}
            Affected people: {request.Affected}

            Instructions:
            1. Assess the urgency based on the information provided.
            2. Return a confidence score between 0 and 100.
            3. Return an urgency score between 1 and 10.
            4. Recommend a suitable humanitarian action.
            5. Do not invent warehouse names, resource locations,
               distances, or available supplies.
            6. For nearestResource, return:
               "Not available - requires live resource data"
               because no live resource database is connected.
            7. AI recommendations are advisory only.
            A human coordinator must make final decisions.
            """;

        var schema = new
        {
            type = "OBJECT",
            properties = new
            {
                confidenceScore = new
                {
                    type = "INTEGER"
                },
                calculatedUrgency = new
                {
                    type = "STRING"
                },
                urgencyScore = new
                {
                    type = "NUMBER"
                },
                recommendedAction = new
                {
                    type = "STRING"
                },
                nearestResource = new
                {
                    type = "STRING"
                }
            },
            required = new[]
            {
                "confidenceScore",
                "calculatedUrgency",
                "urgencyScore",
                "recommendedAction",
                "nearestResource"
            }
        };

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseJsonSchema = schema
            }
        };

        var json = JsonSerializer.Serialize(body);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var url =
    "https://generativelanguage.googleapis.com/v1beta/" +
    "models/gemini-3.5-flash-lite:generateContent";

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post, url);

        httpRequest.Headers.Add("x-goog-api-key", _apiKey);
        httpRequest.Content = content;

        using var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(responseBody);

        var aiText = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(aiText))
        {
            throw new InvalidOperationException(
                "Gemini returned an empty response.");
        }

        var result = JsonSerializer.Deserialize<AIAnalysisResponse>(
            aiText,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null ||
            result.ConfidenceScore < 0 ||
            result.ConfidenceScore > 100 ||
            result.UrgencyScore < 1 ||
            result.UrgencyScore > 10 ||
            string.IsNullOrWhiteSpace(result.CalculatedUrgency) ||
            string.IsNullOrWhiteSpace(result.RecommendedAction))
        {
            throw new InvalidOperationException(
                "Gemini returned an invalid analysis.");
        }

        return result;
    }
}