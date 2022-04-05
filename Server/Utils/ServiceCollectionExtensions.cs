using MQTTnet;
using MQTTnet.Client.Options;
using SmartHomeWWW.Server.Config;
using SmartHomeWWW.Server.Mqtt;

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
                var logger = sp.GetService<ILogger<MqttClientHostedService>>() ?? throw new("ILogger<MqttClientHostedService>");
                return new MqttClientHostedService(logger, new MqttFactory());
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
    }
}
