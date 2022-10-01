namespace SmartHomeWWW.Server.Watchdog;

public class WatchJob
{
    private readonly TimeSpan _timeout;
    private readonly Action _onTimeout;
    private long _time;
    private long _lastTick;

    public bool IsRunning { get; private set; }

    public WatchJob(TimeSpan timeout, Action onTimeout)
    {
        _timeout = timeout;
        _onTimeout = onTimeout;
    }

    public virtual void Init()
    {
    }

    public virtual void Start()
    {
        IsRunning = true;
        _time = _timeout.Ticks;
        _lastTick = DateTime.Now.Ticks;
    }

    public virtual void Stop()
    {
        IsRunning = false;
    }

    public void Tick()
    {
        if (!IsRunning)
        {
            return;
        }

        var ticks = DateTime.Now.Ticks;
        _time -= ticks - _lastTick;
        _lastTick = ticks;

        if (_time <= 0)
        {
            Task.Run(_onTimeout);
            Stop();
        }
    }

    public void Reset()
    {
        if (!IsRunning)
        {
            IsRunning = true;
        }

        _time = _timeout.Ticks;
        _lastTick = DateTime.Now.Ticks;
    }
}
