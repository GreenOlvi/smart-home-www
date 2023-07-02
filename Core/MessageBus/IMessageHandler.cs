namespace SmartHomeWWW.Core.MessageBus;

public interface IMessageHandler<T> where T : IMessage
{
    public Task Handle(T message);
}
