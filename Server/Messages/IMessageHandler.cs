namespace SmartHomeWWW.Server.Messages
{
    public interface IMessageHandler<T> where T : IMessage
    {
        public Task Handle(T message);
    }
}