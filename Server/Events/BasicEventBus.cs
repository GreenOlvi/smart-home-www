using System.Collections;

namespace SmartHomeWWW.Server.Events
{
    public class BasicEventBus : IEventBus
    {
        private readonly Dictionary<string, IList> _handlers = new();

        public void Publish<T>(T @event) where T : IEvent
        {
            var key = GetEventKey<T>();
            if (_handlers.TryGetValue(key, out var list))
            {
                var handlers = (IList<IEventHandler<T>>)list;
                foreach (var handler in handlers)
                {
                    Task.Run(() => handler.Handle(@event));
                }
            }
        }

        public void Subscribe<T>(IEventHandler<T> handler) where T : IEvent
        {
            var key = GetEventKey<T>();
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = new List<IEventHandler<T>>();
                _handlers[key] = list;
            }

            list.Add(handler);
        }

        public void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent
        {
            var key = GetEventKey<T>();
            if (_handlers.TryGetValue(key, out var list))
            {
                list.Remove(handler);
            }
        }

        private static string GetEventKey<T>() where T : IEvent => typeof(T).Name;
    }
}
