using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBot;

public interface ITelegramBotCommand
{
    public Task Run(Message message, CancellationToken cancellationToken = default);
}
