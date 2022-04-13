using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Commands;
using SmartHomeWWW.Server.Messages.Events;

namespace SmartHomeWWW.Server.Jobs
{
    public sealed class TelegramBotJob : IOrchestratorJob,
        IMessageHandler<TelegramMessageReceivedEvent>
    {
        public TelegramBotJob(ILogger<TelegramBotJob> logger, IMessageBus bus, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
        {
            _logger = logger;
            _bus = bus;
            _dbContextFactory = dbContextFactory;
        }

        private readonly ILogger<TelegramBotJob> _logger;
        private readonly IMessageBus _bus;
        private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task Start(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting TelegramBotJob");

            _bus.Subscribe<TelegramMessageReceivedEvent>(this);

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stopping TelegramBotJob");
            return Task.CompletedTask;
        }

        public Task Handle(TelegramMessageReceivedEvent message)
        {
            var text = message.Message.Text;
            if (text == "ping")
            {
                _bus.Publish(new TelegramSendTextMessageCommand(message.ChatId)
                {
                    Text = "pong"
                });
            }

            return Task.CompletedTask;
        }
    }
}
