namespace SmartHomeWWW.Server
{
    public interface IOrchestratorJob : IAsyncDisposable
    {
        public Task Start(CancellationToken cancellationToken);
        public Task Stop(CancellationToken cancellationToken);
    }
}
