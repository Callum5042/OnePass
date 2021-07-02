using System;
using System.Threading.Tasks;

namespace OnePass.CLI.Commands
{
    public class HelpCommand : ICommand
    {
        public CommandType CommandType => CommandType.Help;

        public Task ExecuteAsync(Arguments arguments)
        {
            Console.WriteLine("=== OnePass CLI - Help ===");

            return Task.CompletedTask;
        }
    }
}
