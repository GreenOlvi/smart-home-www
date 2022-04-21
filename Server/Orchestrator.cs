using SmartHomeWWW.Server.Mqtt;
using SmartHomeWWW.Server.Telegram;

namespace SmartHomeWWW.Server
{
    public sealed class Orchestrator : IHostedService, IAsyncDisposable
    {
        public Orchestrator(ILogger<Orchestrator> logger, IServiceProvider sp)
        {
            _logger = logger;

            _jobs = new()
            {
                sp.GetRequiredService<MqttTasmotaAdapter>(),
                sp.GetRequiredService<TelegramBotJob>(),
            };
        }

        private readonly ILogger<Orchestrator> _logger;
        private readonly List<IOrchestratorJob> _jobs;

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
            foreach(var job in _jobs)
            {
                await job.DisposeAsync();
            }
        }

    }
}
