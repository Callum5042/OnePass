using OnePass.Models;
using OnePass.Services;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for LoginPage2.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly ISettingsMonitor _settingsMonitor;

        public LoginPage(ISettingsMonitor settingsMonitor)
        {
            InitializeComponent();
            _settingsMonitor = settingsMonitor;

            Username.Focus();
        }

        private void OnClick_Login(object sender, RoutedEventArgs e)
        {
            var usernameValid = ValidateUsernameSyntax();
            var passwordValid = ValidatePasswordSyntax();

            if (usernameValid && passwordValid)
            {
                var filename = @"usermapping.json";
                if (File.Exists(filename))
                {
                    using var fileRead = File.OpenRead(filename);
                    using var reader = new StreamReader(fileRead);

                    var readJson = reader.ReadToEnd();
                    var accountRoot = JsonSerializer.Deserialize<AccountRoot>(readJson);

                    var account = accountRoot.Accounts.FirstOrDefault(x => x.Username.Equals(Username.Text));
                    if (account is null)
                    {
                        UsernameValidationMessage.Visibility = Visibility.Visible;
                        UsernameValidationMessage.Content = "Username not found.";
                        return;
                    }


                    var hasher = SHA256.Create();
                    var saltedPassword = Password.Password + account.Salt;
                    var hashedPasswordBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                    var hashedPassword = Encoding.UTF8.GetString(hashedPasswordBytes);

                    if (account?.Password == hashedPassword)
                    {
                        var app = Application.Current as App;

                        var window = app.GetService<MainWindow>();
                        window.Show();

                        var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                        loginWindow.Close();

                        return;
                    }
                    else
                    {
                        PasswordValidationMessage.Visibility = Visibility.Visible;
                        PasswordValidationMessage.Content = "Password is invalid.";
                    }
                }
            }
        }

        private void OnTextChanged_ValidateSyntax(object sender, TextChangedEventArgs e)
        {
            ValidateUsernameSyntax();
        }

        private bool ValidateUsernameSyntax()
        {
            var username = Username.Text;

            UsernameValidationMessage.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(username))
            {
                UsernameValidationMessage.Content = "Username is required.";
                return false;
            }
            else
            {
                UsernameValidationMessage.Visibility = Visibility.Collapsed;
                UsernameValidationMessage.Content = string.Empty;
                return true;
            }
        }

        private void OnPasswordChanged_ValidateSyntax(object sender, RoutedEventArgs e)
        {
            ValidatePasswordSyntax();
        }

        private bool ValidatePasswordSyntax()
        {
            var password = Password.Password;

            PasswordValidationMessage.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(password))
            {
                PasswordValidationMessage.Content = "Password is required.";
                return false;
            }
            else
            {
                PasswordValidationMessage.Visibility = Visibility.Collapsed;
                PasswordValidationMessage.Content = string.Empty;

                _settingsMonitor.Current.MasterPassword = password;
                return true;
            }
        }

        private void OnClick_CreateAccount(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.Content = app.GetService<RegisterAccountPage>();
        }
    }
}
