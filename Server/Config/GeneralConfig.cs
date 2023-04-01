namespace SmartHomeWWW.Server.Config;

public record GeneralConfig
{
    public FirmwaresConfig Firmwares { get; set; } = new FirmwaresConfig();
    public MqttConfig Mqtt { get; set; } = new MqttConfig();
    public TasmotaConfig Tasmota { get; set; } = new TasmotaConfig();
    public TelegramConfig Telegram { get; set; } = new TelegramConfig();
}
