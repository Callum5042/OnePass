using Microsoft.Extensions.DependencyInjection;
using OnePass.CLI.Commands;
using OnePass.Services;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace OnePass.CLI
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = BuildServices();
            var service = serviceCollection.BuildServiceProvider();

            try
            {
                var argumentsParser = service.GetRequiredService<ArgumentsParser>();
                var arguments = argumentsParser.Parse(args);

                var resolver = service.GetRequiredService<CommandResolver>();
                var command = resolver.Resolve(arguments.CommandType);
                await command.ExecuteAsync(arguments);
            }
            catch (ArgumentException)
            {
                var helpCommand = service.GetServices<ICommand>().OfType<HelpCommand>().First();
                await helpCommand.ExecuteAsync(new Arguments() { CommandType = CommandType.Help });
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error has occurred. {e.Message}");
            }
        }

        public static ServiceCollection BuildServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<ArgumentsParser>();
            serviceCollection.AddTransient<CommandResolver>();
            serviceCollection.AddTransient<IFileSystem, FileSystem>();

            serviceCollection.AddTransient<ICommand, EncryptCommand>();
            serviceCollection.AddTransient<ICommand, DecryptCommand>();
            serviceCollection.AddTransient<ICommand, HelpCommand>(); 

            serviceCollection.AddTransient<IFileEncryptor, FileEncryptor>();

            return serviceCollection;
        }
    }
}
