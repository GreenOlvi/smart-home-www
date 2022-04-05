namespace SmartHomeWWW.Server.Config
{
    public record struct MqttConfig
    {
        public readonly string Host = string.Empty;
        public readonly int Port = 1883;
        public readonly string? ClientId = null;
    }
}
