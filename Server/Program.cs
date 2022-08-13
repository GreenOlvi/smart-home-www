using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Firmwares;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Mqtt;
using SmartHomeWWW.Server.Persistence;
using SmartHomeWWW.Server.Relays;
using SmartHomeWWW.Server.Telegram;
using SmartHomeWWW.Server.Telegram.Authorisation;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartHomeWWW.Server;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        MapConfig(builder);

        AddServices(builder);

        SetupAuthorization(builder);

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<JsonOptions>(o =>
            {
                o.JsonSerializerOptions.WriteIndented = true;
            });
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseResponseCompression();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TokenMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<SensorsHub>(SensorsHub.RelativePath);
        });

        app.MapRazorPages();
        app.MapControllers();

        app.MapFallbackToFile("index.html");

        app.Run();
    }

    private static void SetupAuthorization(WebApplicationBuilder builder) {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
        });
        builder.Services.AddAuthorization();
    }
    private static void AddServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddSignalR();

        builder.Services.AddSwagger();

        builder.Services.AddAuthorization();

        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
        });

        builder.Services.AddScoped<IFirmwareRepository, DiskFirmwareRepository>();

        builder.Services.AddSingleton<TasmotaClientFactory>();
        builder.Services.AddSingleton<IRelayFactory, RelayFactory>();

        builder.Services.AddDbContextFactory<SmartHomeDbContext>(optionsBuilder =>
            optionsBuilder.UseSqlite(
                builder.Configuration.GetConnectionString("SmartHomeSqliteContext"),
                o => o.MigrationsAssembly("SmartHomeWWW.Server")));

        builder.Services.AddSingleton(sp =>
            new HubConnectionBuilder()
                .WithUrl($"http://localhost:80{SensorsHub.RelativePath}")
                .WithAutomaticReconnect()
                .Build());

        AddHttpClients(builder);

        builder.Services.AddMqttClientHostedService();
        builder.Services.AddTelegramBotHostedService();

        builder.Services.AddSingleton<IMessageBus, BasicMessageBus>();

        builder.Services.AddHostedService<Orchestrator>();
        builder.Services.AddTransient<MqttTasmotaAdapter>();
        builder.Services.AddTransient<TelegramBotJob>();
        builder.Services.AddTransient<WeatherAdapterJob>();

        builder.Services.AddTransient<IAuthorisationService, AuthorisationService>();

        builder.Services.AddTransient<TokenMiddleware>();
        builder.Services.AddSingleton<ITokenRepository, MemoryTokenRepository>();
        builder.Services.AddTransient<ITokenManager, TokenManager>();
    }

    private static void AddSwagger(this IServiceCollection services) =>
        services.AddSwaggerGen(setup =>
        {
            setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1234\""
            });

            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            }
                        },
                        new List<string>()
                    }
                });
        });

    private static void AddHttpClients(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<HttpClient>("Tasmota", client => { client.Timeout = TimeSpan.FromSeconds(5); });
        builder.Services.AddHttpClient<HttpClient>("Telegram");
    }

    private static void MapConfig(WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("secrets.json");

        var generalConfig = new GeneralConfig();
        builder.Configuration.Bind(generalConfig);

        builder.Services.AddSingleton(generalConfig);
        builder.Services.AddSingleton(generalConfig.Firmwares);
        builder.Services.AddSingleton(generalConfig.Mqtt);
        builder.Services.AddSingleton(generalConfig.Telegram);
        builder.Services.AddSingleton(generalConfig.Jwt);
    }
}
