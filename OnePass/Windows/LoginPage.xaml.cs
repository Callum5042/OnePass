﻿using OnePass.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for LoginPage2.xaml
    /// </summary>
    [Inject]
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnClick_Login(object sender, RoutedEventArgs e)
        {
            var usernameValid = ValidateUsernameSyntax();
            var passwordValid = ValidatePasswordSyntax();

            if (usernameValid && passwordValid)
            {
                MessageBox.Show("Verify");
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
                return true;
            }
        }

        private void OnClick_CreateAccount(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            window.Content = app.GetService<RegisterAccountPage>();
        }
    }
}
