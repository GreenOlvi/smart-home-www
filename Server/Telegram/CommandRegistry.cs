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

        public bool TryGetCommand(string keyword, out ITelegramBotCommand command)
        {
            if (!_commands.TryGetValue(keyword, out var cmdType))
            {
                command = NullCommand.Instance;
                return false;
            }

            var cmd = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, cmdType) as ITelegramBotCommand;
            command = cmd ?? NullCommand.Instance;
            return cmd is not null;
        }
    }
}
