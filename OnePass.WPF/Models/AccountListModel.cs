﻿using System;

namespace OnePass.WPF.Models
{
    public class AccountListModel
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public bool Favourite { get; set; }

        public bool MfaEnabled { get; set; }

        public string Notes { get; set; }

        public DateTime? LastChanged { get; set; }
    }
}
