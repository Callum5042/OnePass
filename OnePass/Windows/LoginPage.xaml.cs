﻿using OnePass.Handlers;
using OnePass.Handlers.Interfaces;
using OnePass.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly ILoginHandler _loginHandler;
        private const string _filename = @"usermapping.json";

        public LoginPage(ILoginHandler loginHandler)
        {
            _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));

            InitializeComponent();

            // Set version
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Content = $"v{version.ToString(3)}";
        }

        private async void OnClick_Login(object sender, RoutedEventArgs e)
        {
            var usernameValid = ValidateUsernameSyntax();
            var passwordValid = ValidatePasswordSyntax();

            if (usernameValid && passwordValid)
            {
                var result = await _loginHandler.LoginAsync(Username.Text, Password.Password);

                if (result == LoginResult.Success)
                {
                    // Save username if checked
                    if (RememberUsername.IsChecked == true)
                    {
                        var accountRoot = await ConvertJsonAsync();
                        accountRoot.RememberUsername = Username.Text;

                        await SaveJsonAsync(accountRoot);
                    }

                    // Go to main window
                    var app = Application.Current as App;
                    var window = app.GetService<MainWindow>();
                    window.Show();

                    var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                    loginWindow.Close();
                }
                else if (result == LoginResult.InvalidUsername)
                {
                    UsernameValidationMessage.Visibility = Visibility.Visible;
                    UsernameValidationMessage.Content = "'Username' not found.";
                }
                else if (result == LoginResult.InvalidPassword)
                {
                    PasswordValidationMessage.Visibility = Visibility.Visible;
                    PasswordValidationMessage.Content = "'Password' is invalid.";
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
                UsernameValidationMessage.Content = "'Username' is required.";
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
                PasswordValidationMessage.Content = "'Password' is required.";
                return false;
            }
            else
            {
                PasswordValidationMessage.Visibility = Visibility.Collapsed;
                PasswordValidationMessage.Content = string.Empty;
                return true;
            }
        }

        private void OnClick_CreateAccount(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.Content = app.GetService<RegisterAccountPage>();
        }

        private async void OnLoaded_CheckRemeberUsername(object sender, RoutedEventArgs e)
        {
            // Check if username has been remembered
            if (File.Exists(_filename))
            {
                var accountRoot = await ConvertJsonAsync();
                if (string.IsNullOrWhiteSpace(accountRoot.RememberUsername))
                {
                    Username.Focus();
                }
                else
                {
                    Username.Text = accountRoot.RememberUsername;
                    RememberUsername.IsChecked = true;
                    Password.Focus();
                }
            }
            else
            {
                Username.Focus();
            }
        }

        private static async Task<AccountRoot> ConvertJsonAsync()
        {
            using var fileRead = File.OpenRead(_filename);
            using var reader = new StreamReader(fileRead);

            var readJson = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<AccountRoot>(readJson);
        }

        private static async Task SaveJsonAsync(AccountRoot accountRoot)
        {
            using var file = File.OpenWrite(_filename);
            using var writer = new StreamWriter(file);

            var json = JsonSerializer.Serialize(accountRoot);
            await writer.WriteLineAsync(json);
        }
    }
}
