using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnePass.WPF.Models
{
    [Inject]
    public class AccountModel : ObservableValidator
    {
        private readonly IFileEncoder _fileEncoder;
        private readonly UserData _onePassData;

        public OnePassData OnePassData { get; set; }

        public AccountModel(IFileEncoder fileEncoder, UserData onePassData)
        {
            _fileEncoder = fileEncoder;
            _onePassData = onePassData;

            ErrorsChanged += OnErrorsChanged;
        }

        public async Task LoadAsync()
        {
            OnePassData = await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password, _onePassData.FilePath);
        }

        private void OnErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var error = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            var validationLabel = GetType().GetProperties().FirstOrDefault(x => x.Name == $"{e.PropertyName}Validation");
            validationLabel?.SetValue(this, error);
        }

        public Guid Guid { get; set; }

        [Required]
        public string Name { get => name; set => SetProperty(ref name, value, validate: true); }
        private string name;

        public string Username { get => username; set => SetProperty(ref username, value); }
        private string username;

        [EmailAddress(ErrorMessage = "Not a valid email address.")]
        public string EmailAddress
        {
            get => emailAddress;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ClearErrors(nameof(EmailAddress));
                    emailAddress = null;
                }
                else
                {
                    SetProperty(ref emailAddress, value, validate: true);
                }
            }
        }

        private string emailAddress;

        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public string Website { get => website; set => SetProperty(ref website, value); }
        private string website;

        public bool IsValid()
        {
            ValidateAllProperties();
            return !HasErrors;
        }

        public async Task<Guid> AddAccountAsync()
        {
            var guid = Guid.NewGuid();
            var model = new Account()
            {
                Guid = guid,
                Name = Name,
                Username = Username,
                EmailAddress = EmailAddress,
                Password = Password,
                WebsiteUrl = Website,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                Favourite = Favourite,
                Notes = Notes,
            };

            if (App.Current.AppOptions.EnablePasswordHistory)
            {
                model.PasswordHistory.Add(new PasswordHistory()
                {
                    Guid = Guid.NewGuid(),
                    Password = Password,
                    DateTime = DateTime.Now,
                });
            }

            OnePassData.Accounts.Add(model);

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password, OnePassData, _onePassData.FilePath);
            return guid;
        }

        public async Task UpdateAccountAsync()
        {
            var account = OnePassData.Accounts.First(x => x.Guid == Guid);
            var passwordChanged = account.Password != Password;

            account.Name = Name;
            account.Username = Username;
            account.EmailAddress = EmailAddress;
            account.Password = Password;
            account.DateModified = DateTime.Now;
            account.WebsiteUrl = Website;
            account.Favourite = Favourite;
            account.Notes = Notes;

            if (passwordChanged && App.Current.AppOptions.EnablePasswordHistory)
            {
                account.PasswordHistory.Add(new PasswordHistory()
                {
                    Guid = Guid.NewGuid(),
                    Password = Password,
                    DateTime = DateTime.Now,
                });
            }

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password, OnePassData, _onePassData.FilePath);
        }

        public string NameValidation { get => nameValidation; set => SetProperty(ref nameValidation, value); }
        private string nameValidation;

        public string UsernameValidation { get => usernameValidation; set => SetProperty(ref usernameValidation, value); }

        public void GeneratePassword()
        {
            var generator = new PasswordGenerator()
            {
                HasLowercase = App.Current.AppOptions.Lowercase,
                HasUppercase = App.Current.AppOptions.Uppercase,
                HasNumbers = App.Current.AppOptions.Numbers,
                HasSymbols = App.Current.AppOptions.Symbols,
                MinLength = App.Current.AppOptions.MinLength,
                MaxLength = App.Current.AppOptions.MaxLength
            };

            Password = generator.Generate();
        }

        private string usernameValidation;

        public string EmailAddressValidation { get => emailAddressValidation; set => SetProperty(ref emailAddressValidation, value); }
        private string emailAddressValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;

        public string WebsiteValidation { get => websiteValidation; set => SetProperty(ref websiteValidation, value); }
        private string websiteValidation;

        public IList<PasswordHistoryModel> PasswordHistory { get; set; } = new List<PasswordHistoryModel>();

        public bool Favourite { get => favourite; set => SetProperty(ref favourite, value); }
        private bool favourite;

        public string Notes { get => notes; set => SetProperty(ref notes, value); }
        private string notes;
    }
}
