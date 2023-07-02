using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Authorisation;

namespace SmartHomeWWW.Server.TelegramBotModule;

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

    public static IServiceCollection AddTelegramCommandHandler(this IServiceCollection services) =>
        services.AddTransient<IAuthorisationService, AuthorisationService>()
            .AddHostedService<TelegramBotCommandHandlerJob>();

    public static IServiceCollection AddTelegramLogForwarder(this IServiceCollection services) =>
        services.AddHostedService(sp =>
            new TelegramLogForwarder(
                sp.GetRequiredService<IMessageBus>(),
                sp.GetRequiredService<TelegramConfig>().OwnerId));
}
