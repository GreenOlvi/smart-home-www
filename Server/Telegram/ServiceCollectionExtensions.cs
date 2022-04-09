using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Events;

namespace SmartHomeWWW.Server.Telegram
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelegramBotHostedService(this IServiceCollection services, TelegramConfig config) =>
            services.AddHostedService(sp =>
                new TelegramBotHostedService(
                    sp.GetRequiredService<ILogger<TelegramBotHostedService>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Telegram"),
                    config,
                    sp.GetRequiredService<IEventBus>()));

        public static IServiceCollection AddTelegramBotHostedService(this IServiceCollection services) =>
            services.AddHostedService(sp =>
                new TelegramBotHostedService(
                    sp.GetRequiredService<ILogger<TelegramBotHostedService>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Telegram"),
                    sp.GetRequiredService<TelegramConfig>(),
                    sp.GetRequiredService<IEventBus>()));
    }
}
