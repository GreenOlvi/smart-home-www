using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.Messages.Events;

public record TelegramMessageReceivedEvent : IMessage
{
    public long ChatId { get; init; }
    public long SenderId { get; init; }
    public Message Message { get; init; } = null!;
}
