using ChatSpot.Contracts.Persistence;
using ChatSpot.Models.NoSQL;
using ChatSpot.Models.SQL;
using ChatSpot.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ChatSpot.Configurations;
using Microsoft.EntityFrameworkCore;
public static class PersistenceConfigurations
{
    public static void ConfigurePersistence(this IServiceCollection services)
    {
        services.AddSingleton<MongoDbContext>();
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ChatSpotDbContext>()
            .AddDefaultTokenProviders();
        // todo : add the repositories when done here
        services.AddScoped(typeof(IBaseRepository<>) , typeof(BaseRepository<>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
    }
}