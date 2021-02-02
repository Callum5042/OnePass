using OnePass.Models;
using System;
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
    /// Interaction logic for RegisterAccountPage.xaml
    /// </summary>
    public partial class RegisterAccountPage : Page
    {
        public RegisterAccountPage()
        {
            InitializeComponent();

            Username.Focus();
        }

        private void OnClick_NavigateToLogin(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.Content = app.GetService<LoginPage>();
        }

        private void OnClick_CreateAccount(object sender, RoutedEventArgs e)
        {
            var validUsername = ValidateUsername();
            var validPassword = ValidatePassword();
            var validRepeatPassword = ValidateRepeatPassword();

            if (validUsername && validPassword && validRepeatPassword)
            {
                var filename = @"usermapping.json";
                var accountRoot = new AccountRoot();
                if (File.Exists(filename))
                {
                    using var fileRead = File.OpenRead(filename);
                    using var reader = new StreamReader(fileRead);

                    var readJson = reader.ReadToEnd();
                    accountRoot = JsonSerializer.Deserialize<AccountRoot>(readJson);

                    if (accountRoot.Accounts.Any(x => x.Username.Equals(Username.Text)))
                    {
                        UsernameValidationLabel.Visibility = Visibility.Visible;
                        UsernameValidationLabel.Content = $"Username '{Username.Text}' already exists.";
                        return;
                    }
                }

                var salt = Guid.NewGuid().ToString();
                var hasher = SHA256.Create();

                var saltedPassword = Password.Password + salt;
                var passwordHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                accountRoot.Accounts.Add(new Account()
                {
                    Username = Username.Text,
                    Password = Encoding.UTF8.GetString(passwordHash),
                    Salt = salt,
                    Filename = "filename.bin"
                });

                var json = JsonSerializer.Serialize(accountRoot);
                using var file = File.OpenWrite(filename);
                using var writer = new StreamWriter(file);
                writer.WriteLine(json);

                MessageBox.Show("Account successfully created", "Account Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ValidateUsername()
        {
            var username = Username.Text;

            if (string.IsNullOrEmpty(username))
            {
                UsernameValidationLabel.Visibility = Visibility.Visible;
                UsernameValidationLabel.Content = "Username is required.";
                return false;
            }
            else
            {
                UsernameValidationLabel.Visibility = Visibility.Collapsed;
                UsernameValidationLabel.Content = string.Empty;
                return true;
            }
        }

        private bool ValidatePassword()
        {
            var password = Password.Password;

            if (string.IsNullOrEmpty(password))
            {
                PasswordValidationLabel.Visibility = Visibility.Visible;
                PasswordValidationLabel.Content = "Password is required.";
                return false;
            }
            else
            {
                PasswordValidationLabel.Visibility = Visibility.Collapsed;
                PasswordValidationLabel.Content = string.Empty;
                return true;
            }
        }

        private bool ValidateRepeatPassword()
        {
            var password = Password.Password;
            var repeatPassword = RepeatPassword.Password;

            if (string.IsNullOrEmpty(repeatPassword))
            {
                RepeatPasswordValidationLabel.Visibility = Visibility.Visible;
                RepeatPasswordValidationLabel.Content = "Repeat Password is required.";
                return false;
            }
            else if (repeatPassword != password)
            { 
                RepeatPasswordValidationLabel.Visibility = Visibility.Visible;
                RepeatPasswordValidationLabel.Content = "Repeat Password does not match Password.";
                return false;
            }
            else
            {
                RepeatPasswordValidationLabel.Visibility = Visibility.Collapsed;
                RepeatPasswordValidationLabel.Content = string.Empty;
                return true;
            }
        }

        private void OnTextChanged_ValidateUsernameSyntax(object sender, TextChangedEventArgs e)
        {
            ValidateUsername();
        }
    }
}
