using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Firmwares;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Mqtt;
using SmartHomeWWW.Server.Relays;
using SmartHomeWWW.Server.Relays.Tasmota;
using SmartHomeWWW.Server.Telegram;
using SmartHomeWWW.Server.Telegram.Authorisation;
using SmartHomeWWW.Server.Watchdog;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Server;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.MapConfig();
        AddServices(builder);

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<JsonOptions>(o =>
            {
                o.JsonSerializerOptions.WriteIndented = true;
            });
        }

        var app = builder.Build();

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddProvider(new MessageBusLoggerProvider(app.Services.GetRequiredService<IMessageBus>()));

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

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapHub<SensorsHub>(SensorsHub.RelativePath);

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllersWithViews()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddRazorPages();
        builder.Services.AddSignalR();

        builder.Services.AddSwaggerGen();

        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
        });

        builder.Services.AddScoped<IFirmwareRepository, FileFirmwareRepository>();

        builder.Services.AddSingleton<TasmotaClientFactory>();
        builder.Services.AddSingleton<IRelayFactory, RelayFactory>();

        builder.Services.AddDbContextFactory<SmartHomeDbContext>(optionsBuilder =>
            optionsBuilder.UseSqlite(
                builder.Configuration.GetConnectionString("SmartHomeSqliteContext"),
                o => o.MigrationsAssembly("SmartHomeWWW.Server")));

        builder.Services.AddTransient<IHubConnection, HubConnectionWrapper>();
        builder.Services.AddSingleton(sp => new HubConnectionBuilder()
            .WithUrl($"http://localhost:80{SensorsHub.RelativePath}")
            .WithAutomaticReconnect()
            .Build());

        AddHttpClients(builder);

        builder.Services.AddMqttClientHostedService();
        builder.Services.AddTelegramBotHostedService();

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IMessageBus, BasicMessageBus>();

        builder.Services.AddHostedService<Orchestrator>();
        builder.Services.AddTransient<MqttTasmotaAdapter>();
        builder.Services.AddTransient<TasmotaDeviceUpdaterService>();
        builder.Services.AddTransient<TelegramBotJob>();
        builder.Services.AddTransient(sp => new TelegramLogForwarder(sp.GetRequiredService<IMessageBus>(), sp.GetRequiredService<TelegramConfig>().OwnerId));
        builder.Services.AddTransient<TasmotaRelayHubAdapterJob>();
        builder.Services.AddTransient<WeatherAdapterJob>();
        builder.Services.AddTransient<WatchdogJob>();

        builder.Services.AddSingleton<IKeyValueStore, DbKeyValueStore>();

        builder.Services.AddTransient<IAuthorisationService, AuthorisationService>();
    }

    private static void AddHttpClients(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<HttpClient>("Tasmota", client => { client.Timeout = TimeSpan.FromSeconds(5); });
        builder.Services.AddHttpClient<HttpClient>("Telegram");
    }
}
