﻿using OnePass.Handlers;
using OnePass.Handlers.Interfaces;
using OnePass.Models;
using OnePass.Services.Interfaces;
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
    /// Interaction logic for LoginPage2.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly ISettingsMonitor _settingsMonitor;
        private readonly ILoginHandler _loginHandler;

        public LoginPage(ISettingsMonitor settingsMonitor, ILoginHandler loginHandler)
        {
            _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));

            InitializeComponent();
            _settingsMonitor = settingsMonitor;


            if (!string.IsNullOrEmpty(_settingsMonitor.Current.RememberUsername))
            {
                Username.Text = _settingsMonitor.Current.RememberUsername;
                RememberUsername.IsChecked = true;

                Password.Focus();
            }
            else
            {
                Username.Focus();
            }
        }

        private async void OnClick_Login(object sender, RoutedEventArgs e)
        {
            var usernameValid = ValidateUsernameSyntax();
            var passwordValid = ValidatePasswordSyntax();

            if (usernameValid && passwordValid)
            {
                var result = await _loginHandler.Login(Username.Text, Password.Password);

                if (result == LoginResult.Success)
                {
                    // Save username if checked
                    if (RememberUsername.IsChecked == true)
                    {
                        _settingsMonitor.Current.RememberUsername = Username.Text;
                        _settingsMonitor.SaveAsync().Wait();
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
                    UsernameValidationMessage.Content = "Username not found.";
                }
                else if (result == LoginResult.InvalidPassword)
                {
                    PasswordValidationMessage.Visibility = Visibility.Visible;
                    PasswordValidationMessage.Content = "Password is invalid.";
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
