using System.Reflection;
using ChatSpot.Contracts.Infrastructure;
using ChatSpot.Contracts.Services;
using ChatSpot.Infrastrcutre;
using ChatSpot.Services;


namespace ChatSpot.Configurations;

public static class ServicesConfigurations
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<IJwtTokenGeneration, JwtTokenGeneration>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IChatService, ChatService>();
    }
}