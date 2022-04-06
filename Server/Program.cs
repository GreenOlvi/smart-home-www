using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets.json");

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});

builder.Services.AddScoped<IFirmwareRepository>(sp =>
    new DiskFirmwareRepository(
        sp.GetService<ILogger<DiskFirmwareRepository>>() ?? throw new ArgumentNullException("ILogger<DiskFirmwareRepository>"),
        builder.Configuration.GetValue<string>("FirmwarePath")));

builder.Services.AddSingleton<ITasmotaClientFactory, TasmotaHttpClientFactory>();

builder.Services.AddSingleton<IRelayFactory, RelayFactory>();

builder.Services.AddHttpClient<HttpClient>("Tasmota", client => { client.Timeout = TimeSpan.FromSeconds(5); });

builder.Services.AddDbContextFactory<SmartHomeDbContext>(optionsBuilder =>
    optionsBuilder.UseSqlite(
        builder.Configuration.GetConnectionString("SmartHomeSqliteContext"),
        o => o.MigrationsAssembly("SmartHomeWWW.Server")));

builder.Services.AddSingleton<HubConnection>(sp =>
{
    return new HubConnectionBuilder()
        .WithUrl($"https://localhost:7013{SensorsHub.RelativePath}")
        //.WithUrl($"http://localhost:80{SensorsHub.RelativePath}")
        .WithAutomaticReconnect()
        .Build();
});

// Mqtt client service
var mqttConfig = new MqttConfig();
builder.Configuration.GetRequiredSection("Mqtt").Bind(mqttConfig);
builder.Services.AddMqttClientHostedService(mqttConfig);

// Telegram bot service
var telegramConfig = new TelegramConfig();
builder.Configuration.GetRequiredSection("Telegram").Bind(telegramConfig);
builder.Services.AddHttpClient<HttpClient>("Telegram");
builder.Services.AddTelegramBotHostedService(telegramConfig);

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
