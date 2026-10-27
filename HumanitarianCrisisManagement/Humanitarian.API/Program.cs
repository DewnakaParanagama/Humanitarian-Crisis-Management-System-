
using Humanitarian.Application;
using Humanitarian.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers and JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// CORS configuration for the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8443",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// OpenAPI configuration
builder.Services.AddOpenApi();

var app = builder.Build();

// Global exception handling
app.UseExceptionHandler();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("FrontendPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();