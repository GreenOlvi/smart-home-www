using Microsoft.Extensions.DependencyInjection;
using SmartHomeWWW.Server.TelegramBotModule.BotCommands;

namespace SmartHomeWWW.Server.TelegramBotModule;

public sealed class CommandRegistry(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Dictionary<string, Type> _commands = [];

    public void AddCommand<T>(string keyword) where T : ITelegramBotCommand =>
        _commands.Add(keyword, typeof(T));

    public bool TryGetCommand(string keyword, out Type command)
    {
        var result = _commands.TryGetValue(keyword, out var cmd);
        command = cmd ?? typeof(ITelegramBotCommand);
        return result;
    }

    public bool TryCreateCommandInstance<T>(out ITelegramBotCommand command) where T : ITelegramBotCommand
    {
        var cmd = ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider) as ITelegramBotCommand;
        command = cmd ?? NullCommand.Instance;
        return cmd is not null;
    }

    public bool TryCreateCommandInstance(Type commandType, out ITelegramBotCommand command)
    {
        var cmd = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, commandType) as ITelegramBotCommand;
        command = cmd ?? NullCommand.Instance;
        return cmd is not null;
    }
}
