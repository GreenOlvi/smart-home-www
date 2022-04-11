using SmartHomeWWW.Server.Jobs;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server
{
    public class Orchestrator : IHostedService, IAsyncDisposable
    {
        public Orchestrator(ILogger<Orchestrator> logger, IMessageBus bus, IServiceProvider sp)
        {
            _logger = logger;
            _bus = bus;

            _jobs = new()
            {
                new MqttTasmotaAdapter(
                    sp.GetRequiredService<ILogger<MqttTasmotaAdapter>>(),
                    sp.GetRequiredService<IMessageBus>()),
            };
        }

        private readonly ILogger<Orchestrator> _logger;
        private readonly IMessageBus _bus;
        private readonly List<IOrchestratorJob> _jobs;

        public Task StartAsync(CancellationToken cancellationToken) =>
            Task.WhenAll(_jobs.Select(job => job.Start(cancellationToken)));

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.WhenAll(_jobs.Select(job => job.Stop(cancellationToken)));

        public async ValueTask DisposeAsync()
        {
            foreach(var job in _jobs)
            {
                await job.DisposeAsync();
            }
        }

    }
}
