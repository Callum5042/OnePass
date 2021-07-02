using System;

namespace OnePass.CLI.Commands
{
    public class HelpCommand : ICommand
    {
        public CommandType CommandType => CommandType.Help;

        public void Execute(Arguments arguments)
        {
            Console.WriteLine("=== OnePass CLI - Help ===");
        }
    }
}
