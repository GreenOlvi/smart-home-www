using Microsoft.Extensions.Options;
using MQTTnet.Client;
using SmartHomeWWW.Server.Config;

namespace SmartHomeWWW.Server.Mqtt;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services, Action<MqttClientOptionsBuilder> config)
    {
        services.AddSingleton(sp =>
        {
            var optionsBuilder = new MqttClientOptionsBuilder();
            config(optionsBuilder);
            return optionsBuilder.Build();
        });

        services.AddHostedService<MqttClientHostedService>();
        return services;
    }

    public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services, MqttConfig config) =>
        services.AddMqttClientHostedService(opt =>
        {
            opt.WithTcpServer(config.Host, config.Port);
            if (config.ClientId is not null)
            {
                opt.WithClientId(config.ClientId);
            }
            opt.WithKeepAlivePeriod(TimeSpan.FromMinutes(1));
        });

    public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<MqttConfig>>().Value;
            var opt = new MqttClientOptionsBuilder();
            opt.WithTcpServer(config.Host, config.Port);
            if (config.ClientId is not null)
            {
                opt.WithClientId(config.ClientId);
            }
            return opt.Build();
        });

        services.AddHostedService<MqttClientHostedService>();
        return services;
    }
}
