﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnePass.WPF.Models
{
    [Inject]
    public class AccountModel : ObservableValidator
    {
        private readonly FileEncoder _fileEncoder;
        private readonly OnePassData _onePassData;

        public AccountModel(FileEncoder fileEncoder, OnePassData onePassData)
        {
            _fileEncoder = fileEncoder;
            _onePassData = onePassData;

            ErrorsChanged += OnErrorsChanged;
        }

        public async Task LoadAsync()
        {
            await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password);
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
        public string EmailAddress { get => emailAddress; set => SetProperty(ref emailAddress, value, validate: true); }
        private string emailAddress;

        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public bool IsValid()
        {
            ValidateAllProperties();
            return !HasErrors;
        }

        public async Task<Guid> AddAccountAsync()
        {
            var guid = Guid.NewGuid();
            _fileEncoder.Accounts.Add(new Account()
            {
                Guid = guid,
                Name = Name,
                Username = Username,
                EmailAddress = EmailAddress,
                Password = Password,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
            });

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password);
            return guid;
        }

        public async Task UpdateAccountAsync()
        {
            var account = _fileEncoder.Accounts.First(x => x.Guid == Guid);
            account.Name = Name;
            account.Username = Username;
            account.EmailAddress = EmailAddress;
            account.Password = Password;
            account.DateModified = DateTime.Now;

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password);
        }

        public string NameValidation { get => nameValidation; set => SetProperty(ref nameValidation, value); }
        private string nameValidation;

        public string UsernameValidation { get => usernameValidation; set => SetProperty(ref usernameValidation, value); }

        public void GeneratePassword()
        {
            var generator = new PasswordGenerator();
            Password = generator.Generate();
        }

        private string usernameValidation;

        public string EmailAddressValidation { get => emailAddressValidation; set => SetProperty(ref emailAddressValidation, value); }
        private string emailAddressValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;
    }
}
