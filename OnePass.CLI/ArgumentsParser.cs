using System;

namespace OnePass.CLI
{
    public class ArgumentsParser
    {
        public Arguments Parse(string[] args)
        {
            try
            {
                var commandType = Enum.Parse<CommandType>(args[0].Remove(0, 1), true);
                var file = ParseFile(args);
                var password = ParsePassword(args);

                return new Arguments()
                {
                    CommandType = commandType,
                    File = file,
                    Password = password
                };
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ArgumentException("Could not parse file", e);
            }
        }

        private static string ParseFile(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-file"))
                {
                    var password = args[i + 1];
                    return password.Replace("\"", string.Empty);
                }
            }

            throw new ArgumentException("Could not parse file");
        }

        private static string ParsePassword(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-password"))
                {
                    var password = args[i + 1];
                    return password.Replace("\"", string.Empty);
                }
            }

            throw new ArgumentException("Could not parse file");
        }
    }
}
