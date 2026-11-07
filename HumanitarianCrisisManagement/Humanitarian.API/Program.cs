
using Humanitarian.Application;
using Humanitarian.Infrastructure;
using Humanitarian.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register Global Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// JWT Configuration
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is missing.");

var jwtKey = Encoding.UTF8.GetBytes(jwtSecretKey);

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 bytes long.");
}

// Authentication Configuration
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            // Explicitly configure JWT claim mapping
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

// Role-Based Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CoordinatorOnly", policy =>
        policy.RequireRole("Coordinator"));

    options.AddPolicy("CitizenOnly", policy =>
        policy.RequireRole("Citizen"));

    options.AddPolicy("VolunteerOnly", policy =>
        policy.RequireRole("Volunteer"));

    options.AddPolicy("ShelterAdminOnly", policy =>
        policy.RequireRole("ShelterAdmin"));

    options.AddPolicy("ResourceManagerOnly", policy =>
        policy.RequireRole("ResourceMgr"));
});

// Register Validation Filter
builder.Services.AddScoped<ValidationFilter>();

// Controllers and JSON Configuration
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;

    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8443",
                "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// OpenAPI Configuration
builder.Services.AddOpenApi();

// Build Application
var app = builder.Build();

// Root Endpoint
app.MapGet("/", () => Results.Redirect("/scalar"));

// Global Exception Handling
app.UseExceptionHandler();

// Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Middleware
app.UseCors("FrontendPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Run Application
app.Run();