using OnePass.Handlers;
using OnePass.Handlers.Interfaces;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for RegisterAccountPage.xaml
    /// </summary>
    public partial class RegisterAccountPage : Page
    {
        private readonly IRegisterAccountHandler _registerAccountHandler;

        public RegisterAccountPage(IRegisterAccountHandler registerAccountHandler)
        {
            _registerAccountHandler = registerAccountHandler ?? throw new ArgumentNullException(nameof(registerAccountHandler));
            
            InitializeComponent();
            Username.Focus();
        }

        private void OnClick_NavigateToLogin(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.Content = app.GetService<LoginPage>();
        }

        private async void OnClick_CreateAccount(object sender, RoutedEventArgs e)
        {
            var validUsername = ValidateUsername();
            var validPassword = ValidatePassword();
            var validRepeatPassword = ValidateRepeatPassword();

            if (validUsername && validPassword && validRepeatPassword)
            {
                var result = await _registerAccountHandler.RegisterAccountAsync(Username.Text, Password.Password);
                if (result == RegisterAccountResult.Success)
                {
                    MessageBox.Show("Account successfully created", "Account Created", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (result == RegisterAccountResult.UsernameAlreadyExists)
                {
                    UsernameValidationLabel.Visibility = Visibility.Visible;
                    UsernameValidationLabel.Content = $"Username '{Username.Text}' already exists.";
                }
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
