using System;

namespace OnePass.WPF.Windows
{
    public class AccountListModel
    {
        public string Name { get; set; }

        public string Username { get; set; } = "Username";

        public string EmailAddress { get; set; } = "Email@email.com";

        public string Password { get; set; }

        public bool Favourite { get; set; }

        public bool MfaEnabled { get; set; }

        public string Notes { get; set; }

        public DateTime? LastChanged { get; set; } = DateTime.Now;
    }
}
