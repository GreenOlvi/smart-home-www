namespace SmartHomeWWW.Server.Messages;

public interface IMessageBus
{
    void Publish<T>(T message) where T : IMessage;

    void Subscribe<T>(IMessageHandler<T> handler) where T : IMessage;

    void Unsubscribe<T>(IMessageHandler<T> handler) where T : IMessage;
}
