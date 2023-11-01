using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartHomeWWW.Core.Domain.Repositories;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Firmwares;
using SmartHomeWWW.Server.HealthChecks;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Mqtt;
using SmartHomeWWW.Server.Relays;
using SmartHomeWWW.Server.Relays.Tasmota;
using SmartHomeWWW.Server.Repositories;
using SmartHomeWWW.Server.Sensors;
using SmartHomeWWW.Server.TelegramBotModule;
using SmartHomeWWW.Server.Watchdog;
using SmartHomeWWW.Server.Weather;
using System.IO.Abstractions;

namespace SmartHomeWWW.Server;

internal static class Program
{
    public static async Task Main(string[] args)
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

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseResponseCompression();

            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapHub<SensorsHub>(SensorsHub.RelativePath);

        app.MapHealthChecks("/status", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        await app.RunAsync();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddRazorPages();
        builder.Services.AddSignalR();

        builder.Services.AddSwaggerGen();
        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
        });

        builder.Services.AddScoped<IFirmwareRepository, FileFirmwareRepository>();

        builder.Services.AddSingleton<TasmotaClientFactory>();
        builder.Services.AddSingleton<IRelayFactory, RelayFactory>();

        builder.Services.AddDbContextFactory<SmartHomeDbContext>(optionsBuilder =>
            optionsBuilder.UseSqlite(
                builder.Configuration.GetConnectionString("SmartHomeSqliteContext"),
                o => o.MigrationsAssembly("SmartHomeWWW.Server")));

        builder.Services.AddTransient<IHubConnection, HubConnectionWrapper>();
        builder.Services.AddSingleton(sp =>
        {
            var url = sp.GetRequiredService<IOptions<HubConfig>>().Value.Url;
            return new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
        });

        AddHttpClients(builder);

        builder.Services.AddMqttClientHostedService();

        builder.Services
            .AddTelegramBotHostedService()
            .AddTelegramCommandHandler()
            .AddTelegramLogForwarder(builder.Configuration.GetValue<long>("Telegram:OwnerId"));

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IMessageBus, BasicMessageBus>();

        builder.Services.AddHostedService<Orchestrator>();
        builder.Services.AddTransient<MqttTasmotaAdapter>();
        builder.Services.AddTransient<TasmotaDeviceUpdaterService>();
        builder.Services.AddTransient<TasmotaRelayHubAdapterJob>();
        builder.Services.AddTransient<WeatherAdapterJob>();
        builder.Services.AddTransient<WatchdogJob>();
        builder.Services.AddTransient<SensorMonitorJob>();

        builder.Services.AddScoped<IWeatherReportRepository, WeatherReportRepository>();

        builder.Services.AddSingleton<IKeyValueStore, DbKeyValueStore>();

        AddHealthChecks(builder);
    }

    private static void AddHttpClients(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<HttpClient>("Tasmota", client => { client.Timeout = TimeSpan.FromSeconds(5); });
        builder.Services.AddHttpClient<HttpClient>("Telegram");
    }

    private static void AddHealthChecks(WebApplicationBuilder builder) =>
        builder.Services.AddHealthChecks()
            .AddCheck<DbHealthCheck>("db-check", timeout: TimeSpan.FromSeconds(30));
}
