using ChatSpot.Configurations;
using ChatSpot.Hubs;
using ChatSpot.Models.SQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
    options.AddPolicy("AllowReact", p =>
        p.WithOrigins(
                "http://localhost:5173",
                "https://chatspot-liart.vercel.app"
            )
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
builder.Services.AddSignalR();


var app = builder.Build();


app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.UseCors("SignalRPolicy");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
