using Telegram.Bot.Types.Enums;

namespace SmartHomeWWW.Server.Messages.Commands;

public record TelegramSendTextMessageCommand : IMessage
{
    public long ChatId { get; init; }
    public string Text { get; init; } = string.Empty;
    public ParseMode? ParseMode { get; init; }
    public bool? DisableWebPagePreview { get; init; }
    public bool? DisableNotification { get; init; }
    public int? ReplyToMessageId { get; init; }
    public bool? AllowSendingWithoutReply { get; init; }
}
