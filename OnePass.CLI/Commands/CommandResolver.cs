using System.Collections.Generic;
using System.Linq;

namespace OnePass.CLI.Commands
{
    public class CommandResolver
    {
        private readonly IEnumerable<ICommand> _commands;

        public CommandResolver(IEnumerable<ICommand> commands)
        {
            _commands = commands;
        }

        public ICommand Resolve(CommandType commandType)
        {
            var command = _commands.FirstOrDefault(x => x.CommandType == commandType);
            return command ?? new HelpCommand();
        }
    }
}
