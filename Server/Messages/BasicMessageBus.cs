using System.Collections;

namespace SmartHomeWWW.Server.Messages;

public class BasicMessageBus : IMessageBus
{
    // TODO: use concurrent collections
    private readonly Dictionary<string, IList> _handlers = new ();

    public void Publish<T>(T message) where T : IMessage
    {
        var key = GetMessageKey<T>();
        if (_handlers.TryGetValue(key, out var list))
        {
            var handlers = (IList<IMessageHandler<T>>)list;
            foreach (var handler in handlers)
            {
                Task.Run(() => handler.Handle(message));
            }
        }
    }

    public void Subscribe<T>(IMessageHandler<T> handler) where T : IMessage
    {
        var key = GetMessageKey<T>();
        if (!_handlers.TryGetValue(key, out var list))
        {
            list = new List<IMessageHandler<T>>();
            _handlers[key] = list;
        }

        list.Add(handler);
    }

    public void Unsubscribe<T>(IMessageHandler<T> handler) where T : IMessage
    {
        var key = GetMessageKey<T>();
        if (_handlers.TryGetValue(key, out var list))
        {
            list.Remove(handler);
        }
    }

    private static string GetMessageKey<T>() where T : IMessage => typeof(T).Name;
}
