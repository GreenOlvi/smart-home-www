namespace SmartHomeWWW.Server.TelegramBotModule;

public record TelegramConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public long OwnerId { get; set; }
}
