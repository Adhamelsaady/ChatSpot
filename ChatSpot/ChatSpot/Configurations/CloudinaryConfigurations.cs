namespace ChatSpot.Configurations;
using CloudinaryDotNet;
public static class CloudinaryConfigurations
{
    public static void ConfigureCloudinary(this IServiceCollection services, IConfiguration configuration)
    {
        var cloudinaryConfig = configuration.GetSection("Cloudinary");
        var cloudinary = new Cloudinary(new Account(
            cloudinaryConfig["CloudName"],
            cloudinaryConfig["ApiKey"],
            cloudinaryConfig["ApiSecret"]
        ));
        cloudinary.Api.Secure = true;
        services.AddSingleton(cloudinary);
    }
}