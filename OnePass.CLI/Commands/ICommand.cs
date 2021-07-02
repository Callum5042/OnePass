using System.Threading.Tasks;

namespace OnePass.CLI.Commands
{
    public interface ICommand
    {
        public CommandType CommandType { get; }

        public Task ExecuteAsync(Arguments arguments);
    }
}
