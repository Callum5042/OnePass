using System;

namespace OnePass.WPF.Models
{
    public class PasswordHistoryModel
    {
        public string Password { get; set; }

        public DateTime DateSet { get; set; }

        public string DateSetText => $"Set {DateSet:g}";
    }
}
