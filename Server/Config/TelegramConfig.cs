namespace SmartHomeWWW.Server.Config
{
    public record TelegramConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public long OwnerId { get; set; }
    }
}
