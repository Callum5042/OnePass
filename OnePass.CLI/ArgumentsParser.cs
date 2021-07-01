using System;

namespace OnePass.CLI
{
    public class ArgumentsParser
    {
        public Arguments Parse(string[] args)
        {
            var commandType = Enum.Parse<CommandType>(args[0].Remove(0, 1), true);

            return new Arguments()
            {
                CommandType = commandType,
                File = args[2]
            };
        }
    }
}
