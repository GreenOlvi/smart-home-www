using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Firmwares;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Mqtt;
using SmartHomeWWW.Server.Relays;
using SmartHomeWWW.Server.Telegram;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        MapConfig(builder);

        AddServices(builder);

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseResponseCompression();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<SensorsHub>(SensorsHub.RelativePath);
        });

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddSignalR();

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
        {
            return new HubConnectionBuilder()
                .WithUrl($"http://localhost:80{SensorsHub.RelativePath}")
                .WithAutomaticReconnect()
                .Build();
        });

        AddHttpClients(builder);

        builder.Services.AddMqttClientHostedService();
        builder.Services.AddTelegramBotHostedService();

        builder.Services.AddSingleton<IMessageBus, BasicMessageBus>();
        builder.Services.AddSingleton<AddressBook>();

        builder.Services.AddHostedService<Orchestrator>();
        builder.Services.AddTransient<MqttTasmotaAdapter>();
        builder.Services.AddTransient<TelegramBotJob>();
    }

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
    }
}

