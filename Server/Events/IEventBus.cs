namespace SmartHomeWWW.Server.Events
{
    public interface IEventBus
    {
        void Publish<T>(T @event) where T : IEvent;

        void Subscribe<T>(IEventHandler<T> handler) where T : IEvent;

        void Unsubscribe<T>(IEventHandler<T> handler) where T : IEvent;
    }
}
