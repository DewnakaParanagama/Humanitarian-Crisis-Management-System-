using Humanitarian.Application.Common.Interfaces;
using Humanitarian.Infrastructure.Persistence;
using Humanitarian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Humanitarian.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
        "DefaultConnection")
        ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");


    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(
            provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAssistanceRequestService, AssistanceRequestService>();

        services.AddScoped<ICoordinatorService, CoordinatorService>();

        services.AddScoped<IShelterService, ShelterService>();

        services.AddScoped<IVolunteerService, VolunteerService>();

        services.AddHttpClient<IAIAnalysisService, GeminiAIAnalysisService>();

        return services;
    }


}
