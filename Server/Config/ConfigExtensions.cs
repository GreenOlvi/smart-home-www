namespace SmartHomeWWW.Server.Config;

public static class ConfigExtensions
{
    public static WebApplicationBuilder MapConfig(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("secrets.json");

        var generalConfig = new GeneralConfig();
        builder.Configuration.Bind(generalConfig);

        builder.Services.AddSingleton(generalConfig);
        builder.Services.AddSingleton(generalConfig.Firmwares);
        builder.Services.AddSingleton(generalConfig.Mqtt);
        builder.Services.AddSingleton(generalConfig.Tasmota);
        builder.Services.AddSingleton(generalConfig.Tasmota.Discovery);
        builder.Services.AddSingleton(generalConfig.Telegram);

        return builder;
    }
}
