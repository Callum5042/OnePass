using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnePass.WPF.Models
{
    public class AccountListModel : ObservableObject
    {
        private const string NotSet = "Not set";

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
                    OnPropertyChanged(nameof(HasUsername));
                    OnPropertyChanged(nameof(UsernameText));
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
                    OnPropertyChanged(nameof(HasEmailAddress));
                    OnPropertyChanged(nameof(EmailAddressText));
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

        public bool HasUsername => NormalizedOrNull(Username) != null;

        public string UsernameText => NormalizedOrNull(Username) ?? NotSet;

        public bool HasEmailAddress => NormalizedOrNull(EmailAddress) != null;

        public string EmailAddressText => NormalizedOrNull(EmailAddress) ?? NotSet;

        public bool HasPassword => !string.IsNullOrEmpty(Password);

        public bool HasWebsite => NormalizedOrNull(WebsiteUrl) != null;

        public string WebsiteText => NormalizedOrNull(WebsiteUrl) ?? NotSet;

        private static string NormalizedOrNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public string Password
        {
            get => password;
            set
            {
                if (SetProperty(ref password, value))
                {
                    OnPropertyChanged(nameof(HasPassword));
                }
            }
        }
        private string password;

        public bool MfaEnabled { get; set; }

        public DateTime? DateCreated { get => dateCreated; set => SetProperty(ref dateCreated, value); }
        private DateTime? dateCreated;

        public DateTime? DateModified { get => dateModified; set => SetProperty(ref dateModified, value); }
        private DateTime? dateModified;

        public IList<PasswordHistoryModel> PasswordHistory { get; set; } = new List<PasswordHistoryModel>();

        public bool Favourite { get => favourite; set => SetProperty(ref favourite, value); }
        private bool favourite;

        public string Notes { get => notes; set => SetProperty(ref notes, value); }
        private string notes;

        public string WebsiteUrl
        {
            get => websiteUrl;
            set
            {
                if (SetProperty(ref websiteUrl, value))
                {
                    OnPropertyChanged(nameof(HasWebsite));
                    OnPropertyChanged(nameof(WebsiteText));
                }
            }
        }
        private string websiteUrl;
    }
}
