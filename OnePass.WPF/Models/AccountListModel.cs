using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace OnePass.WPF.Models
{
    public class AccountListModel : ObservableObject
    {
        public Guid Guid { get; set; }

        public string Name { get => name; set => SetProperty(ref name, value); }
        private string name;

        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public bool Favourite { get; set; }

        public bool MfaEnabled { get; set; }

        public string Notes { get; set; }

        public DateTime? LastChanged { get; set; }
    }
}
