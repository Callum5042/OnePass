using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.WPF.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace OnePass.WPF.Models
{
    public class LoginModel : ObservableValidator
    {
        private readonly FileEncoder _fileEncoder;

        public LoginModel()
        {
            _fileEncoder = new FileEncoder();

            ErrorsChanged += OnErrorsChanged;
        }

        private void OnErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var error = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            var validationLabel = GetType().GetProperties().FirstOrDefault(x => x.Name == $"{e.PropertyName}Validation");
            validationLabel?.SetValue(this, error);
        }

        [Required]
        public string Username { get => username; set => SetProperty(ref username, value, validate: true); }
        private string username;

        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters.")]
        public string Password { get => password; set => SetProperty(ref password, value, validate: true); }
        private string password;

        public string UsernameValidation { get => usernameValidation; set => SetProperty(ref usernameValidation, value); }
        private string usernameValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;

        public (bool valid, bool fileNotFound, bool invalidPassword) IsValid()
        {
            ValidateAllProperties();

            // Has UX errors
            if (HasErrors)
            {
                return (false, false, false);
            }

            // Check if file exists
            if (!File.Exists($"{Username}.bin"))
            {
                UsernameValidation = $"File {Username}.bin could not be found.";
                return (false, true, false);
            }

            // Verify password
            try
            {
                if (!_fileEncoder.Verify(Username, Password))
                {
                    PasswordValidation = "Password is incorrect.";
                    return (false, false, true);
                }
            }
            catch (InvalidOperationException)
            {
                UsernameValidation = $"{Username}.bin is not a valid OnePass file.";
                return (false, true, false);
            }

            return (true, false, false);
        }
    }
}
