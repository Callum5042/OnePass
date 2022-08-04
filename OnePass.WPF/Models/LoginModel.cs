using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.WPF.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.WPF.Models
{
    [Inject]
    public class LoginModel : ObservableValidator
    {
        private readonly FileEncoder _fileEncoder;

        public LoginModel(FileEncoder fileEncoder)
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
            // Save options
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(Path.Combine(appdata, "OnePass"));
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            if (File.Exists(path))
            {
                AppOptions options = null;
                using (var file = File.OpenRead(path))
                {
                    options = await JsonSerializer.DeserializeAsync<AppOptions>(file);
                }

                using (var file = File.Open(path, FileMode.Truncate))
                {
                    options.RememberUsername = Login.RememberMe ? Login.Username : string.Empty;
                    await JsonSerializer.SerializeAsync(file, options);
                }
            }
            else
            {
                using var file = File.Create(path);
                await JsonSerializer.SerializeAsync(file, new AppOptions
                {
                    RememberUsername = Login.RememberMe ? Login.Username : string.Empty
                });
            }
        }
    }
}
