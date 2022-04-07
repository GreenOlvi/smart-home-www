using MQTTnet;
using MQTTnet.Client.Options;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Events;
using SmartHomeWWW.Server.Mqtt;
using SmartHomeWWW.Server.Telegram;

namespace SmartHomeWWW.Server.Utils
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services, Action<MqttClientOptionsBuilder> config) =>
            services.AddHostedService(sp =>
            {
                var optionsBuilder = new MqttClientOptionsBuilder();
                config(optionsBuilder);
                var options = optionsBuilder.Build();
                var logger = sp.GetRequiredService<ILogger<MqttClientHostedService>>();
                return new MqttClientHostedService(logger, new MqttFactory(), options);
            });

        public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services, MqttConfig config) =>
            services.AddMqttClientHostedService(opt =>
            {
                opt.WithTcpServer(config.Host, config.Port);
                if (config.ClientId is not null)
                {
                    opt.WithClientId(config.ClientId);
                }
            });

        public static IServiceCollection AddTelegramBotHostedService(this IServiceCollection services, TelegramConfig config) =>
            services.AddHostedService(sp =>
                new TelegramBotHostedService(
                    sp.GetRequiredService<ILogger<TelegramBotHostedService>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Telegram"),
                    config,
                    sp.GetRequiredService<IEventBus>()));

    }
}
