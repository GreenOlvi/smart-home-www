using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Core.MessageBus.Events;

namespace SmartHomeWWW.Server.Infrastructure;

public sealed class MessageBusLoggerProvider : ILoggerProvider
{
    private readonly IMessageBus _messageBus;

    public MessageBusLoggerProvider(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public ILogger CreateLogger(string categoryName) => new MessageBusLogger(_messageBus, categoryName);

    public void Dispose()
    {
    }

    public class MessageBusLogger : ILogger
    {
        private readonly IMessageBus _messageBus;
        private readonly string _category;

        public MessageBusLogger(IMessageBus messageBus, string category)
        {
            _messageBus = messageBus;
            _category = category;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NoopDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            _messageBus.Publish(LogEvent.Log(_category, logLevel, eventId, formatter(state, exception)));

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
