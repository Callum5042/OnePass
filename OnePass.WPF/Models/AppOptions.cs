namespace OnePass.WPF.Models
{
    public class AppOptions
    {
        public string RememberUsername { get; set; }

        public int? WindowWidth { get; set; }

        public int? WindowHeight { get; set; }

        public int? WindowPositionX { get; set; }

        public int? WindowPositionY { get; set; }

        public bool WindowMaximized { get; set; }

        public bool Lowercase { get; set; } = true;

        public bool Uppercase { get; set; } = true;

        public bool Numbers { get; set; } = true;

        public bool Symbols { get; set; } = true;

        public int MinLength { get; set; } = 5;

        public int MaxLength { get; set; } = 10;
    }
}
