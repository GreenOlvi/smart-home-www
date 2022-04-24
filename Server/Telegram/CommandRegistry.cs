using SmartHomeWWW.Server.Telegram.BotCommands;

namespace SmartHomeWWW.Server.Telegram
{
    public class CommandRegistry
    {
        public CommandRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _commands = new();

        public void AddCommand<T>(string keyword) where T : ITelegramBotCommand =>
            _commands.Add(keyword, typeof(T));

        public bool TryGetCommand(string keyword, out Type command)
        {
            var result = _commands.TryGetValue(keyword, out var cmd);
            command = cmd ?? typeof(ITelegramBotCommand);
            return result;
        }

        public bool TryCreateCommandInstance<T>(out ITelegramBotCommand command) where T : ITelegramBotCommand
        {
            var cmd = ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider) as ITelegramBotCommand;
            command = cmd ?? NullCommand.Instance;
            return cmd is not null;
        }

        public bool TryCreateCommandInstance(Type commandType, out ITelegramBotCommand command)
        {
            var cmd = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, commandType) as ITelegramBotCommand;
            command = cmd ?? NullCommand.Instance;
            return cmd is not null;
        }
    }
}
