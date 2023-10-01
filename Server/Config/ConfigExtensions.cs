using SmartHomeWWW.Server.TelegramBotModule;

namespace SmartHomeWWW.Server.Config;

public static class ConfigExtensions
{
    public static WebApplicationBuilder MapConfig(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("secrets.json");

        builder.Services.AddOptions<MqttConfig>()
            .Bind(builder.Configuration.GetRequiredSection(nameof(GeneralConfig.Mqtt)));

        builder.Services.AddOptions<FirmwaresConfig>()
            .Bind(builder.Configuration.GetRequiredSection(nameof(GeneralConfig.Firmwares)));

        builder.Services.AddOptions<HubConfig>()
            .Bind(builder.Configuration.GetRequiredSection(nameof(GeneralConfig.Hub)));

        builder.Services.AddOptions<TasmotaDiscoveryConfig>()
            .Bind(builder.Configuration.GetSection(nameof(GeneralConfig.Tasmota))
                    .GetSection(nameof(TasmotaConfig.Discovery)));

        builder.Services.AddOptions<TelegramConfig>()
            .Bind(builder.Configuration.GetRequiredSection(nameof(GeneralConfig.Telegram)));

        return builder;
    }
}
