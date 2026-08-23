using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.WPF.Models
{
    [Inject]
    public class LoginModel : ObservableValidator
    {
        private readonly IFileEncoder _fileEncoder;

        public LoginModel(IFileEncoder fileEncoder)
        {
            _fileEncoder = fileEncoder;

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

        public async Task<string> CreateAccountAsync(string username, string password)
        {
            var filePath = Path.GetFullPath($"{username}.bin");
            await _fileEncoder.SaveAsync(username, password, new OnePassData(), filePath);
            return filePath;
        }

        public async Task<OnePassData> TryDecryptAsync()
        {
            Login.FilePathValidation = null;
            Login.PasswordValidation = null;

            var username = Path.GetFileNameWithoutExtension(Login.FilePath);

            try
            {
                if (!_fileEncoder.Verify(username, Login.Password, Login.FilePath))
                {
                    Login.PasswordValidation = "Password is incorrect.";
                    return null;
                }

                var data = await _fileEncoder.LoadAsync(username, Login.Password, Login.FilePath);
                if (data is null)
                {
                    Login.FilePathValidation = "Not a valid OnePass file.";
                }

                return data;
            }
            catch (FileNotFoundException)
            {
                Login.FilePathValidation = $"File {Login.FileName} could not be found.";
            }
            catch (DirectoryNotFoundException)
            {
                Login.FilePathValidation = $"File {Login.FileName} could not be found.";
            }
            catch (UnauthorizedAccessException)
            {
                Login.FilePathValidation = "Unable to open the selected file.";
            }
            catch (EndOfStreamException)
            {
                Login.FilePathValidation = "Not a valid OnePass file.";
            }
            catch (InvalidOperationException)
            {
                Login.FilePathValidation = "Not a valid OnePass file.";
            }
            catch (CryptographicException)
            {
                Login.FilePathValidation = "Not a valid OnePass file.";
            }
            catch (JsonException)
            {
                Login.FilePathValidation = "Not a valid OnePass file.";
            }
            catch (ArgumentException)
            {
                Login.FilePathValidation = "Not a valid OnePass file.";
            }
            catch (IOException)
            {
                Login.FilePathValidation = "Unable to open the selected file.";
            }

            return null;
        }

        public async Task LoadOptions()
        {
            // Check if selected
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            if (File.Exists(path))
            {
                using var file = File.OpenRead(path);
                var options = await JsonSerializer.DeserializeAsync<AppOptions>(file) ?? new AppOptions();

                ApplyOptions(options);

                App.Current.AppOptions = options;
            }
        }

        public void ApplyOptions(AppOptions options, string currentDirectory = null)
        {
            var rememberedFilePath = options.RememberFilePath;

            if (string.IsNullOrWhiteSpace(rememberedFilePath) && !string.IsNullOrWhiteSpace(options.RememberUsername))
            {
                var baseDirectory = currentDirectory ?? Directory.GetCurrentDirectory();
                rememberedFilePath = Path.Combine(baseDirectory, $"{options.RememberUsername}.bin");
            }

            if (string.IsNullOrWhiteSpace(rememberedFilePath))
            {
                return;
            }

            try
            {
                rememberedFilePath = Path.GetFullPath(rememberedFilePath);
            }
            catch (Exception exception) when (exception is ArgumentException || exception is NotSupportedException)
            {
                return;
            }

            if (!File.Exists(rememberedFilePath))
            {
                return;
            }

            Login.FilePath = rememberedFilePath;
            Login.RememberMe = true;
            options.RememberFilePath = rememberedFilePath;
            options.RememberUsername = string.Empty;
        }

        public async Task SaveOptions()
        {
            // Save options
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(Path.Combine(appdata, "OnePass"));
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            var options = App.Current.AppOptions ?? new AppOptions();
            UpdateRememberedFileOption(options);
            App.Current.AppOptions = options;

            using var file = File.Create(path);
            await JsonSerializer.SerializeAsync(file, options);
        }

        public void UpdateRememberedFileOption(AppOptions options)
        {
            options.RememberFilePath = Login.RememberMe ? Path.GetFullPath(Login.FilePath) : string.Empty;
            options.RememberUsername = string.Empty;
        }
    }
}
