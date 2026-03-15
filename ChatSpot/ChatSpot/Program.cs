using ChatSpot.Configurations;
using ChatSpot.Hubs;
using ChatSpot.Models.SQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
    options.AddPolicy("AllowReact", p =>
        p.WithOrigins("http://localhost:4000")
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();


// then later:
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
