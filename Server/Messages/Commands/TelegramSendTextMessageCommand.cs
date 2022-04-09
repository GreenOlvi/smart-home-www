namespace SmartHomeWWW.Server.Messages.Commands
{
    public record TelegramSendTextMessageCommand : IMessage
    {
        public long ChatId { get; }
        public string Text { get; init; } = string.Empty;

        public TelegramSendTextMessageCommand(long chatId)
        {
            ChatId = chatId;
        }
    }
}
