using System.Collections.Generic;

namespace OnePass.Services
{
    public class PasswordGeneratorOptions
    {
        public bool Numbers { get; set; }

        public bool Lowercase { get; set; }

        public bool Uppercase { get; set; }

        public bool Symbols { get; set; }

        public int MinLength { get; set; }

        public int MaxLength { get; set; }

        public IEnumerable<char> ExcludeSymbolList { get; set; }
    }
}
