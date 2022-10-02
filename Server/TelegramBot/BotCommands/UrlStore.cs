using SmartHomeWWW.Server.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using Telegram.Bot.Types;

namespace SmartHomeWWW.Server.TelegramBot.BotCommands;

public class UrlStore : ITelegramBotCommand
{
    private readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(30);

    private readonly IKeyValueStore _store;
    private readonly IMessageBus _bus;

    public UrlStore(IKeyValueStore store, IMessageBus bus)
    {
        _store = store;
        _bus = bus;
    }

    public async Task Run(Message message, CancellationToken cancellationToken = default)
    {
        var key = $"TelegramMessageUrl-{message.Chat.Id}-{message.From?.Id}-{message.MessageId}";
        var url = message.EntityValues?.FirstOrDefault();
        if (url is null)
        {
            return;
        }

        await _store.AddValueAsync(key, new StoredUrl
        {
            From = message.From?.Id,
            Url = url,
            Message = message,
        }, DefaultExpiration);

        var expireAt = DateTime.UtcNow + DefaultExpiration;
        _bus.Publish(new TelegramSendTextMessageCommand
        {
            ChatId = message.Chat.Id,
            ReplyToMessageId = message.MessageId,
            Text = $"Stored until {expireAt.ToLocalTime()}",
        });
    }

    public record StoredUrl
    {
        public long? From { get; init; }
        public string Url { get; init; } = string.Empty;
        public Message? Message { get; init; }
    }
}
