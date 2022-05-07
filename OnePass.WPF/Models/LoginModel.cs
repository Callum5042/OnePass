using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.WPF.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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
                    var model = validationContext.ObjectInstance as LoginValidationModel;
                    if (File.Exists($"{model.Username}.bin"))
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

        [Required]
        [FileExists]
        public string Username { get => username; set => SetProperty(ref username, value); }
        private string username;

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

        private sealed class FileExistsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (File.Exists($"{value}.bin"))
                {
                    return new ValidationResult($"File {value}.bin already exists");
                }

                return ValidationResult.Success;
            }
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

    public class LoginModel : ObservableValidator
    {
        private readonly FileEncoder _fileEncoder;

        public LoginModel()
        {
            _fileEncoder = new FileEncoder();

            ErrorsChanged += OnErrorsChanged;
        }

        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        private void OnErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var error = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            var validationLabel = GetType().GetProperties().FirstOrDefault(x => x.Name == $"{e.PropertyName}Validation");
            validationLabel?.SetValue(this, error);
        }

        public LoginValidationModel Login { get; set; } = new LoginValidationModel();

        public RegisterValidationModel Register { get; set; } = new RegisterValidationModel();

        public async Task CreateAccountAsync(string username, string password)
        {
            await _fileEncoder.SaveAsync(username, password);
        }

        public async Task LoadOptions()
        {
            // Check if selected
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            if (File.Exists(path))
            {
                using var file = File.OpenRead(path);
                var options = await JsonSerializer.DeserializeAsync<AppOptions>(file);

                if (!string.IsNullOrEmpty(options.RememberUsername))
                {
                    Login.Username = options.RememberUsername;
                    Login.RememberMe = true;
                }
            }
        }

        public async Task SaveOptions()
        {
            var options = new AppOptions
            {
                RememberUsername = Login.RememberMe ? Login.Username : string.Empty
            };

            // Save options
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(Path.Combine(appdata, "OnePass"));
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            using var file = File.Create(path);
            await JsonSerializer.SerializeAsync(file, options);
        }
    }
}
