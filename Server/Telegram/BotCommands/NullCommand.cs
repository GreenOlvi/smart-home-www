using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram.BotCommands;

public class NullCommand : ITelegramBotCommand
{
    public Task Run(Message message, CancellationToken cancellationToken) => Task.CompletedTask;

    public static NullCommand Instance { get; } = new ();
}
