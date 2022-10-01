using SmartHomeWWW.Server.Messages;

namespace SmartHomeWWW.Server.Watchdog;

public sealed class WatchdogJob : IOrchestratorJob
{
    private readonly ILogger<WatchdogJob> _logger;
    private readonly List<WatchJob> _jobs;
    private Timer? _timer;
    private bool _didInit;

    public WatchdogJob(ILogger<WatchdogJob> logger, ILoggerFactory loggerFactory, IMessageBus bus)
    {
        _logger = logger;

        // TODO: load list from settings
        _jobs = new List<WatchJob>
        {
            new MqttWatchJob(loggerFactory.CreateLogger<MqttWatchJob>(),
                "rfbridge/OpenMQTTGateway_ESP8266_RF-CC1101/SYStoMQTT",
                TimeSpan.FromSeconds(120 * 1.5),
                () => _logger.LogError("RFBridge missed keepalive message"),
                bus),
        };
    }

    public Task Start(CancellationToken cancellationToken)
    {
        if (!_didInit)
        {
            _jobs.ForEach(j => j.Init());
            _didInit = true;
        }

        _jobs.ForEach(j => j.Start());
        _timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _jobs.ForEach(j => j.Stop());

        _timer?.Dispose();
        _timer = null;

        return Task.CompletedTask;
    }

    private void OnTick(object? state)
    {
        _jobs.ForEach(j => j.Tick());
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var job in _jobs.OfType<IAsyncDisposable>())
        {
            await job.DisposeAsync();
        }

        if (_timer is not null)
        {
            await _timer.DisposeAsync();
        }
    }
}
