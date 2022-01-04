using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});

builder.Services.AddScoped<IFirmwareRepository>(sp =>
    new DiskFirmwareRepository(
        sp.GetService<ILogger<DiskFirmwareRepository>>(),
        builder.Configuration.GetValue<string>("FirmwarePath")));

builder.Services.AddDbContextFactory<SmartHomeDbContext>(optionsBuilder =>
    optionsBuilder.UseSqlite(
        builder.Configuration.GetConnectionString("SmartHomeSqliteContext"),
        o => o.MigrationsAssembly("SmartHomeWWW.Server")));

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


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
