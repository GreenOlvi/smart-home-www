using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Telegram;

public interface ITelegramBotCommand
{
    public Task Run(Message message, CancellationToken cancellationToken = default);
}
