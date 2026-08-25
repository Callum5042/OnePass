using System;

namespace OnePass.Models
{
    public class PasswordHistory
    {
        public Guid Guid { get; set; }

        public string Password { get; set; }

        public DateTime DateTime { get; set; }
    }
}
