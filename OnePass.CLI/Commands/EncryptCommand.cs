using System;

namespace OnePass.CLI.Commands
{
    public class EncryptCommand : ICommand
    {
        public CommandType CommandType => CommandType.Encrypt;

        public void Execute(Arguments arguments)
        {
            throw new NotImplementedException();
        }
    }
}
