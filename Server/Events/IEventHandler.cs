namespace SmartHomeWWW.Server.Events
{
    public interface IEventHandler<T> where T : IEvent
    {
        public Task Handle(T @event);
    }
}