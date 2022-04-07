using SmartHomeWWW.Server.Events;

namespace SmartHomeWWW.Server.Telegram
{
    public class TelegramSendTextMessageCommand : IEvent
    {
        public long ChatId { get; }
        public string Text { get; init; } = string.Empty;

        public TelegramSendTextMessageCommand(long chatId)
        {
            ChatId = chatId;
        }
    }
}
