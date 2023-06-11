using SmartHomeWWW.Server.Relays.Tasmota;
using SmartHomeWWW.Server.Sensors;
using SmartHomeWWW.Server.Telegram;
using SmartHomeWWW.Server.Watchdog;
using SmartHomeWWW.Server.Weather;

namespace SmartHomeWWW.Server;

public sealed class Orchestrator : IHostedService, IAsyncDisposable
{
    public Orchestrator(ILogger<Orchestrator> logger, IServiceProvider sp)
    {
        _logger = logger;

        _scope = sp.CreateScope();

        _jobs = new ()
        {
            sp.GetRequiredService<MqttTasmotaAdapter>(),
            sp.GetRequiredService<TasmotaRelayHubAdapterJob>(),
            sp.GetRequiredService<TelegramBotJob>(),
            sp.GetRequiredService<TelegramLogForwarder>(),
            _scope.ServiceProvider.GetRequiredService<WeatherAdapterJob>(),
            sp.GetRequiredService<WatchdogJob>(),
            sp.GetRequiredService<SensorMonitorJob>(),
        };
    }

    private readonly ILogger<Orchestrator> _logger;
    private readonly List<IOrchestratorJob> _jobs;
    private readonly IServiceScope _scope;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting Orchestrator");
        return Task.WhenAll(_jobs.Select(job => job.Start(cancellationToken)));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping Orchestrator");
        return Task.WhenAll(_jobs.Select(job => job.Stop(cancellationToken)));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var job in _jobs)
        {
            await job.DisposeAsync();
        }
        _scope.Dispose();
    }
}
