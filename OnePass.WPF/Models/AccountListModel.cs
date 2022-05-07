using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace OnePass.WPF.Models
{
    public class AccountListModel : ObservableObject
    {
        public Guid Guid { get; set; }

        public string Name { get => name; set => SetProperty(ref name, value); }
        private string name;

        public string Username { get => username; set => SetProperty(ref username, value); }
        private string username;

        public string EmailAddress { get => emailAddress; set => SetProperty(ref emailAddress, value); }
        private string emailAddress;

        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public bool Favourite { get; set; }

        public bool MfaEnabled { get; set; }

        public string Notes { get; set; }

        public DateTime? DateModified { get => dateModified; set => SetProperty(ref dateModified, value); }
        private DateTime? dateModified;
    }
}
