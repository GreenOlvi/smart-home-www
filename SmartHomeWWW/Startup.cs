using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartHomeCore.Infrastructure;
using SmartHomeWWW.Hubs;
using System.Linq;

namespace SmartHomeWWW
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSignalR();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
            });

            services.AddScoped<SmartHomeCore.Firmwares.IFirmwareRepository>(sp =>
                new SmartHomeCore.Firmwares.DiskFirmwareRepository(
                    sp.GetService<ILogger<SmartHomeCore.Firmwares.DiskFirmwareRepository>>(),
                    Configuration.GetValue<string>("FirmwarePath")));

            services.AddDbContextFactory<SmartHomeDbContext>(optionsBuilder =>
                optionsBuilder.UseSqlite(
                    Configuration.GetConnectionString("SmartHomeSqliteContext"),
                    o => o.MigrationsAssembly("SmartHomeWWW")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("firmware", "update/firmware.bin");
                endpoints.MapControllers();

                endpoints.MapBlazorHub();
                endpoints.MapHub<SensorsHub>(SensorsHub.RelativePath);
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
