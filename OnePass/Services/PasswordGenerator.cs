using OnePass.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnePass.Services
{
    [Inject(typeof(IPasswordGenerator))]
    public class PasswordGenerator : IPasswordGenerator
    {
        private readonly IList<char> _lowercase;
        private readonly IList<char> _uppercase;
        private readonly IList<char> _numbers;
        private readonly IList<char> _symbols;

        public PasswordGenerator()
        {
            _lowercase = new List<char>() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            _uppercase = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            _numbers = new List<char>() { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            _symbols = new List<char>() { '[', ']', '{', '}', ';', ':', '\'', '@', '#', '~', ',', '<', '.', '>', '/', '?' };
        }

        public string Generate(PasswordGeneratorOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Create character list
            var charList = GetCharList(options).ToList();
            

            // Generate password
            var random = new Random(); //< Should this be injected?
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < random.Next(options.MinLength, options.MaxLength); i++)
            {
                var rnd = random.Next(0, charList.Count);
                var c = charList[rnd];

                stringBuilder.Append(c);
            }

            // Verify
            var password = stringBuilder.ToString();
            if (options.Symbols && !password.Any(x => char.IsSymbol(x)))
            {
                var symbolList = _symbols.ToList();
                if (options.ExcludeSymbolList != null)
                {
                    symbolList = symbolList.Except(options.ExcludeSymbolList).ToList();
                }

                var rnd = random.Next(0, password.Length);
                var c = password[rnd];

                rnd = random.Next(0, symbolList.Count);
                password = password.Replace(c, symbolList[rnd]);
            }

            if (options.Lowercase && !password.Any(x => char.IsLower(x)))
            {
                var lowercaseList = _lowercase.ToList();
                var rnd = random.Next(0, password.Length);
                var c = password[rnd];

                rnd = random.Next(0, lowercaseList.Count);
                password = password.Replace(c, lowercaseList[rnd]);
            }

            if (options.Uppercase && !password.Any(x => char.IsUpper(x)))
            {
                var uppercaseList = _uppercase.ToList();
                var rnd = random.Next(0, password.Length);
                var c = password[rnd];

                rnd = random.Next(0, uppercaseList.Count);
                password = password.Replace(c, uppercaseList[rnd]);
            }

            if (options.Numbers && !password.Any(x => char.IsDigit(x)))
            {
                var numbersList = _numbers.ToList();
                var rnd = random.Next(0, password.Length);
                var c = password[rnd];

                rnd = random.Next(0, numbersList.Count);
                password = password.Replace(c, numbersList[rnd]);
            }

            return password;
        }

        private IEnumerable<char> GetCharList(PasswordGeneratorOptions options)
        {
            IEnumerable<char> list = new List<char>();

            if (options.Lowercase)
            {
                list = list.Union(_lowercase);
            }

            if (options.Uppercase)
            {
                list = list.Union(_uppercase);
            }

            if (options.Numbers)
            {
                list = list.Union(_numbers);
            }

            if (options.Symbols)
            {
                list = list.Union(_symbols);

                if (options.ExcludeSymbolList != null)
                {
                    list = list.Except(options.ExcludeSymbolList);
                }
            }

            return list;
        }
    }
}
