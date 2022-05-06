using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.WPF.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    [Inject]
    public class AccountModel : ObservableValidator
    {
        private readonly FileEncoder _fileEncoder;

        public AccountModel()
        {
            _fileEncoder = new FileEncoder();
            _fileEncoder.Load();
        }

        public ICommand Command { get; set; }

        public Guid Guid { get; set; }

        [Required]
        public string Name { get => name; set => SetProperty(ref name, value, validate: true); }
        private string name;

        public string Username { get => username; set => SetProperty(ref username, value); }
        private string username;

        [EmailAddress(ErrorMessage = "Not a valid email address.")]
        public string EmailAddress { get => emailAddress; set => SetProperty(ref emailAddress, value, validate: true); }
        private string emailAddress;

        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public bool IsValid()
        {
            ValidateAllProperties();
            NameValidation = FirstError(nameof(Name));
            EmailAddressValidation = FirstError(nameof(EmailAddress));

            return !HasErrors;
        }

        public string FirstError(string property)
        {
            return GetErrors(property).Select(x => x.ErrorMessage).FirstOrDefault();
        }

        public Guid AddAccount()
        {
            var guid = Guid.NewGuid();
            _fileEncoder.Accounts.Add(new Account()
            {
                Guid = guid,
                Name = Name,
            });

            _fileEncoder.Save();
            return guid;
        }

        public void RegisterAccount()
        {
            var account = _fileEncoder.Accounts.First(x => x.Guid == Guid);
            account.Name = Name;

            _fileEncoder.Save();
        }

        public string NameValidation { get => nameValidation; set => SetProperty(ref nameValidation, value); }
        private string nameValidation;

        public string UsernameValidation { get => usernameValidation; set => SetProperty(ref usernameValidation, value); }
        private string usernameValidation;

        public string EmailAddressValidation { get => emailAddressValidation; set => SetProperty(ref emailAddressValidation, value); }
        private string emailAddressValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;
    }
}
