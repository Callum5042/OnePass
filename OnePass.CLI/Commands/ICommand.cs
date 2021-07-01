namespace OnePass.CLI.Commands
{
    public interface ICommand
    {
        public CommandType CommandType { get; }

        public void Execute(Arguments arguments);
    }
}
