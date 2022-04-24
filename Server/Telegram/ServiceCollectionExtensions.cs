using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Messages;

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
                    sp.GetRequiredService<IMessageBus>(),
                    sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>()));

        public static IServiceCollection AddTelegramBotHostedService(this IServiceCollection services) =>
            services.AddHostedService(sp =>
                new TelegramBotHostedService(
                    sp.GetRequiredService<ILogger<TelegramBotHostedService>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Telegram"),
                    sp.GetRequiredService<TelegramConfig>(),
                    sp.GetRequiredService<IMessageBus>(),
                    sp.GetRequiredService<IDbContextFactory<SmartHomeDbContext>>()));
    }
}
