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
            Console.WriteLine("-[encrypt/decrypt] -file \"filename.bin\" -password \"pass\"");

            Console.WriteLine("= Encryprt Example =");
            Console.WriteLine("-decrypt -file \"filename.bin\" -password \"password\"");
            Console.WriteLine("= Decrypt Example =");
            Console.WriteLine("-encrypt -file \"filename.bin\" -password \"password\"");

            return Task.CompletedTask;
        }
    }
}
