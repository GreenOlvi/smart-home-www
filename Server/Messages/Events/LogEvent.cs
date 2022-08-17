namespace SmartHomeWWW.Server.Messages.Events;

public record LogEvent : IMessage
{
    public LogLevel Level { get; init; } = LogLevel.Trace;
    public EventId EventId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static LogEvent Log(string category, LogLevel logLevel, EventId eventId, string message) => new()
    {
        Level = logLevel,
        EventId = eventId,
        Category = category,
        Message = message,
    };
}
