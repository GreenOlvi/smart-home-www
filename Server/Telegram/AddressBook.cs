using SmartHomeWWW.Server.Config;

namespace SmartHomeWWW.Server.Telegram;

public class AddressBook
{
    public AddressBook(TelegramConfig config)
    {
        OwnerId = config.OwnerId;
    }

    public long OwnerId { get; }
}
