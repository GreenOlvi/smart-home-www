namespace SmartHomeWWW.Server.Config;

public record MqttConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1883;
    public string? ClientId { get; set; }
}
