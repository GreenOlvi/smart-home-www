using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using SmartHomeWWW.Client;
using SmartHomeWWW.Client.HttpClients;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

        builder.Services.AddHttpClient("base", client => client.BaseAddress = baseAddress);
        builder.Services.AddHttpClient<FirmwareHttpClient>("base");
        builder.Services.AddHttpClient<SensorsHttpClient>("base");
        builder.Services.AddHttpClient<RelaysHttpClient>("base");
        builder.Services.AddHttpClient<WeatherHttpClient>("base");

        builder.Services.AddSingleton(sp =>
        {
            var nav = sp.GetRequiredService<NavigationManager>();
            return new HubConnectionBuilder()
                .WithUrl(nav.ToAbsoluteUri("/sensorshub"))
                .WithAutomaticReconnect()
                .Build();
        });

        await builder.Build().RunAsync();
    }
}
