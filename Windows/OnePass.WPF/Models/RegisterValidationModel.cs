using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace OnePass.WPF.Models
{
    public class RegisterValidationModel : ObservableValidator
    {
        public RegisterValidationModel()
        {
            ErrorsChanged += OnErrorsChanged;
        }

        private void OnErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var error = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            var validationLabel = GetType().GetProperties().FirstOrDefault(x => x.Name == $"{e.PropertyName}Validation");
            validationLabel?.SetValue(this, error);
        }

        [Required(ErrorMessage = "The Filename field is required.")]
        public string FilePath
        {
            get => filePath;
            set
            {
                if (SetProperty(ref filePath, value))
                {
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }
        private string filePath;

        [Required]
        public string FileName => Path.GetFileName(FilePath);

        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters.")]
        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        [Required(ErrorMessage = "The Repeat Password field is required.")]
        [RepeatPassword]
        public string RepeatPassword { get => repeatPassword; set => SetProperty(ref repeatPassword, value); }
        private string repeatPassword;

        public string UsernameValidation { get => usernameValidation; set => SetProperty(ref usernameValidation, value); }
        private string usernameValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;

        public string RepeatPasswordValidation { get => repeatPasswordValidation; set => SetProperty(ref repeatPasswordValidation, value); }
        private string repeatPasswordValidation;

        public bool IsValid()
        {
            ValidateAllProperties();

            // Has UX errors
            if (HasErrors)
            {
                return false;
            }

            return true;
        }

        private sealed class RepeatPasswordAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (validationContext.ObjectInstance is RegisterValidationModel model)
                {
                    if (model.Password != model.RepeatPassword)
                    {
                        return new ValidationResult("Passwords do not match.");
                    }
                }

                return ValidationResult.Success;
            }
        }
    }
}
