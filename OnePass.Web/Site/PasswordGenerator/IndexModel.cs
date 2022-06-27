﻿namespace OnePass.Web.Site.PasswordGenerator
{
    public class IndexModel
    {
        public int Amount { get; set; } = 5;

        public int MinLength { get; set; } = 5;

        public int MaxLength { get; set; } = 10;

        public bool Uppercase { get; set; } = true;

        public bool Lowercase { get; set; } = true;

        public bool Numbers { get; set; } = true;

        public bool Symbols { get; set; }

        public string? Passwords { get; set; }
    }
}