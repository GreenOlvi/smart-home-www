namespace SmartHomeWWW.Server;

public interface IOrchestratorJob : IAsyncDisposable
{
    public Task Start(CancellationToken cancellationToken = default);
    public Task Stop(CancellationToken cancellationToken = default);
}
