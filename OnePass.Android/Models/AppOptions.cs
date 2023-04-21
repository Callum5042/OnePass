namespace OnePass.Droid.Models
{
    public class AppOptions
    {
        public string RememberUsername { get; set; }

        public bool Lowercase { get; set; } = true;

        public bool Uppercase { get; set; } = true;

        public bool Numbers { get; set; } = true;

        public bool Symbols { get; set; } = true;

        public int MinLength { get; set; } = 5;

        public int MaxLength { get; set; } = 10;

        public bool EnablePasswordHistory { get; set; } = true;
    }
}