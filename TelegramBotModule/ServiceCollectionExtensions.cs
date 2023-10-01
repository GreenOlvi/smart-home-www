using Microsoft.Extensions.DependencyInjection;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.TelegramBotModule.Authorisation;

namespace SmartHomeWWW.Server.TelegramBotModule;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBotHostedService(this IServiceCollection services) =>
        services.AddHostedService<TelegramBotHostedService>();

    public static IServiceCollection AddTelegramCommandHandler(this IServiceCollection services) =>
        services.AddTransient<IAuthorisationService, AuthorisationService>()
            .AddHostedService<TelegramBotCommandHandlerJob>();

    public static IServiceCollection AddTelegramLogForwarder(this IServiceCollection services, long userId) =>
        services.AddHostedService(sp => new TelegramLogForwarder(sp.GetRequiredService<IMessageBus>(), userId));
}
