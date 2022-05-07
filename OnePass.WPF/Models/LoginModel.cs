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
        [FileExists]
        public string Username { get => username; set => SetProperty(ref username, value); }
        private string username;

        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters.")]
        [CheckPassword]
        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public string UsernameValidation { get => usernameValidation; set => SetProperty(ref usernameValidation, value); }
        private string usernameValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;

        [Required]
        public string RegisterUsername { get => registerUsername; set => SetProperty(ref registerUsername, value, validate: true); }
        private string registerUsername;

        public bool IsValid()
        {
            ValidateAllProperties();

            // Has UX errors
            if (HasErrors)
            {
                return false;
            }

            // Verify password
            //try
            //{
            //    if (!_fileEncoder.Verify(Username, Password))
            //    {
            //        PasswordValidation = "Password is incorrect.";
            //        return (false, false, true);
            //    }
            //}
            //catch (InvalidOperationException)
            //{
            //    UsernameValidation = $"{Username}.bin is not a valid OnePass file.";
            //    return (false, true, false);
            //}

            return true;
        }

        private sealed class FileExistsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (!File.Exists($"{value}.bin"))
                {
                    return new ValidationResult($"File {value}.bin could not be found.");
                }

                return ValidationResult.Success;
            }
        }

        private sealed class CheckPasswordAttribute : ValidationAttribute
        {
            private readonly FileEncoder _fileEncoder;

            public CheckPasswordAttribute()
            {
                _fileEncoder = new FileEncoder();
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                try
                {
                    var model = validationContext.ObjectInstance as LoginModel;
                    if (File.Exists($"{value}.bin"))
                    {
                        if (!_fileEncoder.Verify(model.Username, model.Password))
                        {
                            return new ValidationResult("Password is incorrect.");
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // This might be worth checking elsewhere
                    return new ValidationResult("Not a valid OnePass file.");
                }

                return ValidationResult.Success;
            }
        }
    }
}
