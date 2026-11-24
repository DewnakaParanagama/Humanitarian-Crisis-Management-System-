using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Application.Features.AI.DTOs;
using Humanitarian.Domain.Common;
using Humanitarian.Domain.Entities;
using Humanitarian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Humanitarian.Tests;

public class GeminiAIAnalysisServiceTests
{
    [Theory]
    [InlineData(null, "English")]
    [InlineData(" ", "English")]
    [InlineData("EN", "English")]
    [InlineData("sInHaLa", "Sinhala")]
    [InlineData("si", "Sinhala")]
    [InlineData("TA", "Tamil")]
    public void Language_is_normalized(string? input, string expected)
    {
        var request = new AnalyzeRequestDto { Language = input! };

        Assert.Equal(expected, request.Language);
        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("French")]
    [InlineData("Spanish")]
    public void Unsupported_language_fails_dto_validation(string language)
    {
        var request = new AnalyzeRequestDto { Language = language };

        Assert.Contains(Validate(request), error => error.MemberNames.Contains(nameof(request.Language)));
    }

    [Fact]
    public async Task Unsupported_language_is_rejected_by_service()
    {
        await using var context = CreateContext();
        var service = CreateService(context, new RecordingHandler(ValidGeminiResponse()));

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.AnalyzeRequestAsync(
            new AnalyzeRequestDto { Language = "French" }, CancellationToken.None));

        Assert.Contains("Language", exception.Message);
    }

    [Fact]
    public async Task Prompt_contains_only_live_shelters_and_resources_sorted_by_distance()
    {
        await using var context = CreateContext();
        context.Shelters.AddRange(
            new Shelter { Name = "Nearest Open", Location = "A", District = "D1", Latitude = 6.9271, Longitude = 79.8612, TotalCapacity = 10, CurrentOccupancy = 2, Status = ShelterStatus.Open },
            new Shelter { Name = "Farther Open", Location = "B", District = "D2", Latitude = 7.2906, Longitude = 80.6337, TotalCapacity = 10, CurrentOccupancy = 1, Status = ShelterStatus.Open },
            new Shelter { Name = "Full Open", Location = "C", District = "D3", Latitude = 6.93, Longitude = 79.86, TotalCapacity = 5, CurrentOccupancy = 5, Status = ShelterStatus.Open },
            new Shelter { Name = "Closed Available", Location = "D", District = "D4", Latitude = 6.93, Longitude = 79.86, TotalCapacity = 10, CurrentOccupancy = 1, Status = ShelterStatus.Closed });
        context.Resources.AddRange(
            new Resource { ItemName = "Water", Category = "Relief", Quantity = 20, Unit = "bottles", Location = "A", Status = "Available" },
            new Resource { ItemName = "Rice", Category = "Food", Quantity = 10, Unit = "kg", Location = "B", Status = "Available" },
            new Resource { ItemName = "Expired", Category = "Other", Quantity = 100, Unit = "units", Location = "C", Status = "Unavailable" },
            new Resource { ItemName = "Empty", Category = "Other", Quantity = 0, Unit = "units", Location = "D", Status = "Available" });
        await context.SaveChangesAsync();

        var handler = new RecordingHandler(ValidGeminiResponse());
        var service = CreateService(context, handler);
        await service.AnalyzeRequestAsync(new AnalyzeRequestDto { Latitude = 6.9271, Longitude = 79.8612 }, CancellationToken.None);

        Assert.Contains("Nearest Open", handler.RequestBody);
        Assert.Contains("Farther Open", handler.RequestBody);
        Assert.DoesNotContain("Full Open", handler.RequestBody);
        Assert.DoesNotContain("Closed Available", handler.RequestBody);
        Assert.Contains("Water", handler.RequestBody);
        Assert.Contains("Rice", handler.RequestBody);
        Assert.DoesNotContain("Expired", handler.RequestBody);
        Assert.DoesNotContain("Empty", handler.RequestBody);
        Assert.True(handler.RequestBody.IndexOf("Nearest Open", StringComparison.Ordinal) < handler.RequestBody.IndexOf("Farther Open", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Gemini_json_response_is_parsed()
    {
        await using var context = CreateContext();
        var service = CreateService(context, new RecordingHandler(ValidGeminiResponse()));

        var result = await service.AnalyzeRequestAsync(new AnalyzeRequestDto(), CancellationToken.None);

        Assert.Equal("Summary", result.Summary);
        Assert.Equal(84, result.ConfidenceScore);
        Assert.Equal("High", result.CalculatedUrgency);
        Assert.Equal(8, result.UrgencyScore);
        Assert.Equal("Dispatch support", result.RecommendedAction);
        Assert.Equal("Nearest Open", result.NearestResource);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"candidates\":[]}")]
    public async Task Malformed_or_failed_Gemini_response_returns_fallback(string responseBody)
    {
        await using var context = CreateContext();
        var service = CreateService(context, new RecordingHandler(responseBody, HttpStatusCode.BadGateway));

        var result = await service.AnalyzeRequestAsync(new AnalyzeRequestDto(), CancellationToken.None);

        Assert.Equal(0, result.ConfidenceScore);
        Assert.Equal("Unknown", result.CalculatedUrgency);
        Assert.Equal(1, result.UrgencyScore);
        Assert.NotEmpty(result.Summary);
        Assert.NotEmpty(result.RecommendedAction);
        Assert.NotEmpty(result.NearestResource);
    }

    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }

    private static GeminiAIAnalysisService CreateService(TestDbContext context, HttpMessageHandler handler) =>
        new(new HttpClient(handler), context, new TestConfiguration());

    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static string ValidGeminiResponse() =>
        JsonSerializer.Serialize(new
        {
            candidates = new[]
            {
                new { content = new { parts = new[] { new { text = "{\"summary\":\"Summary\",\"confidenceScore\":84,\"calculatedUrgency\":\"High\",\"urgencyScore\":8,\"recommendedAction\":\"Dispatch support\",\"nearestResource\":\"Nearest Open\"}" } } } }
            }
        });

    private sealed class RecordingHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class TestConfiguration : IConfiguration
    {
        public string? this[string key]
        {
            get => key == "Gemini:ApiKey" ? "test-key" : null;
            set { }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new TestConfigurationSection(key, this[key]);
    }

    private sealed class TestConfigurationSection(string key, string? value) : IConfigurationSection
    {
        public string? this[string key] { get => null; set { } }
        public string Key { get; } = key;
        public string Path => Key;
        public string? Value { get; set; } = value;
        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new TestConfigurationSection(key, null);
    }
}

internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Crisis> Crises => Set<Crisis>();
    public DbSet<AssistanceRequest> AssistanceRequests => Set<AssistanceRequest>();
    public DbSet<Shelter> Shelters => Set<Shelter>();
    public DbSet<VolunteerProfile> VolunteerProfiles => Set<VolunteerProfile>();
    public DbSet<VolunteerAssignment> VolunteerAssignments => Set<VolunteerAssignment>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();

    public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        base.SaveChangesAsync(cancellationToken);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken).ContinueWith(task => (IDbContextTransaction)task.Result, cancellationToken);
}
