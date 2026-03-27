using ChatSpot.Configurations;
using ChatSpot.Hubs;
using ChatSpot.Models.SQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Must match the React dev origin (Vite uses 4000 in web/package.json; 5173 is Vite default).
var reactOrigins = new[]
{
    "http://localhost:4000",
    "http://localhost:5125",
    "http://localhost:5173",
    "https://chatspot-liart.vercel.app",
};

builder.Services.AddCors(options =>
    options.AddPolicy("AllowReact", p =>
        p.WithOrigins(reactOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ChatSpotDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.ConfigureServices();
builder.Services.ConfigurePersistence();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureCloudinary(builder.Configuration);
builder.Services.AddSignalR();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
