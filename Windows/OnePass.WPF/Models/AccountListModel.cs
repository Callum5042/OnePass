using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnePass.WPF.Models
{
    public class AccountListModel : ObservableObject
    {
        public Guid Guid { get; set; }

        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                    OnPropertyChanged(nameof(Initial));
                }
            }
        }
        private string name;

        public string Username
        {
            get => username;
            set
            {
                if (SetProperty(ref username, value))
                {
                    OnPropertyChanged(nameof(DisplayLogin));
                }
            }
        }
        private string username;

        public string EmailAddress
        {
            get => emailAddress;
            set
            {
                if (SetProperty(ref emailAddress, value))
                {
                    OnPropertyChanged(nameof(DisplayLogin));
                }
            }
        }
        private string emailAddress;

        public string DisplayName => NormalizedOrNull(Name) ?? "Unnamed account";

        public string DisplayLogin => NormalizedOrNull(Username) ?? NormalizedOrNull(EmailAddress) ?? "No login details";

        public string Initial
        {
            get
            {
                var letter = NormalizedOrNull(Name)?.FirstOrDefault(char.IsLetterOrDigit) ?? default;
                return letter == default ? "?" : char.ToUpper(letter).ToString();
            }
        }

        private static string NormalizedOrNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public bool MfaEnabled { get; set; }

        public DateTime? DateModified { get => dateModified; set => SetProperty(ref dateModified, value); }
        private DateTime? dateModified;

        public IList<PasswordHistoryModel> PasswordHistory { get; set; } = new List<PasswordHistoryModel>();

        public bool Favourite { get => favourite; set => SetProperty(ref favourite, value); }
        private bool favourite;

        public string Notes { get => notes; set => SetProperty(ref notes, value); }
        private string notes;

        public string WebsiteUrl { get => websiteUrl; set => SetProperty(ref websiteUrl, value); }
        private string websiteUrl;
    }
}
