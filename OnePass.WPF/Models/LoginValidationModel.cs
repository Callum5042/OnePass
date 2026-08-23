using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OnePass.WPF.Models
{
    public class LoginValidationModel : ObservableValidator
    {
        public LoginValidationModel()
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
        [FileExists]
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

        public string FileName => Path.GetFileName(FilePath);

        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters.")]
        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public string FilePathValidation { get => filePathValidation; set => SetProperty(ref filePathValidation, value); }
        private string filePathValidation;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;

        public bool RememberMe { get => rememberMe; set => SetProperty(ref rememberMe, value); }
        private bool rememberMe;

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

        private sealed class FileExistsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var filePath = value as string;
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return ValidationResult.Success;
                }

                if (!File.Exists(filePath))
                {
                    return new ValidationResult($"File {Path.GetFileName(filePath)} could not be found.");
                }

                return ValidationResult.Success;
            }
        }
    }
}
