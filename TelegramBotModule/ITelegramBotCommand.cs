using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBotModule;

public interface ITelegramBotCommand
{
    public Task Run(Message message, CancellationToken cancellationToken = default);
}
