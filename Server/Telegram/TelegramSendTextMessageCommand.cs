using SmartHomeWWW.Server.Messages;

namespace SmartHomeWWW.Server.Telegram
{
    public class TelegramSendTextMessageCommand : IMessage
    {
        public long ChatId { get; }
        public string Text { get; init; } = string.Empty;

        public TelegramSendTextMessageCommand(long chatId)
        {
            ChatId = chatId;
        }
    }
}
