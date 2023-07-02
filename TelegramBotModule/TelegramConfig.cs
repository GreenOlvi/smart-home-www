namespace SmartHomeWWW.Server.TelegramBotModule;

public record TelegramConfig
{
    public string ApiKey { get; init; } = null!;
    public long OwnerId { get; init; }
    public string? HttpClientName { get; init; }
}
