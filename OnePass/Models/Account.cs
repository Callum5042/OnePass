using System;
using System.Collections.Generic;

namespace OnePass.Models
{
    public class Account
    {
        public Guid Guid { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public bool Favourite { get; set; }

        public string WebsiteUrl { get; set; }

        public bool MfaEnabled { get; set; }

        public string Notes { get; set; }

        public IList<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();
    }
}
