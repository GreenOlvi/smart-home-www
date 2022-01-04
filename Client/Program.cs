using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartHomeWWW.Client;
using SmartHomeWWW.Client.Repositories;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });
builder.Services.AddHttpClient<FirmwareHttpClient>(client => client.BaseAddress = baseAddress);
builder.Services.AddHttpClient<SensorsHttpClient>(client => client.BaseAddress = baseAddress);

await builder.Build().RunAsync();
