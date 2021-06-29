using OnePass.Handlers.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private readonly IChangePasswordHandler _changePasswordHandler;

        public ChangePasswordWindow(IChangePasswordHandler changePasswordHandler)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            ShowInTaskbar = false;

            _changePasswordHandler = changePasswordHandler ?? throw new ArgumentNullException(nameof(changePasswordHandler));

            OldPasswordTextbox.Focus();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var isValid = Validate();

            if (isValid)
            {
                var message = await _changePasswordHandler.ChangePassword(OldPasswordTextbox.Password, NewPasswordTextbox.Password);
                if (message)
                {
                    MessageBox.Show("Password has been changed");
                    Close();
                }
                else
                {
                    OldPasswordValidationMessage.Content = "Old Password is incorrect.";
                    OldPasswordValidationMessage.Visibility = Visibility.Visible;
                }
            }
        }

        private bool Validate()
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(OldPasswordTextbox.Password))
            {
                OldPasswordValidationMessage.Content = "'Old Password' is required.";
                OldPasswordValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordTextbox.Password))
            {
                NewPasswordValidationMessage.Content = "'New Password' is required.";
                NewPasswordValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(RepeatPasswordTextbox.Password))
            {
                RepeatPasswordValidationMessage.Content = "'Repeat Password' is required.";
                RepeatPasswordValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (NewPasswordTextbox.Password != RepeatPasswordTextbox.Password)
            {
                RepeatPasswordValidationMessage.Content = "The passwords do not match.";
                RepeatPasswordValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void OnPasswordChanged_OldPasswordTextbox(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OldPasswordTextbox.Password))
            {
                OldPasswordValidationMessage.Content = "'Old Password' is required.";
                OldPasswordValidationMessage.Visibility = Visibility.Visible;
            }
            else
            {
                OldPasswordValidationMessage.Content = string.Empty;
                OldPasswordValidationMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPasswordChanged_NewPasswordText(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewPasswordTextbox.Password))
            {
                NewPasswordValidationMessage.Content = "'New Password' is required.";
                NewPasswordValidationMessage.Visibility = Visibility.Visible;
            }
            else
            {
                NewPasswordValidationMessage.Content = string.Empty;
                NewPasswordValidationMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPasswordChanged_RepeatPasswordTextbox(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RepeatPasswordTextbox.Password))
            {
                RepeatPasswordValidationMessage.Content = "'Repeat Password' is required.";
                RepeatPasswordValidationMessage.Visibility = Visibility.Visible;
            }
            else
            {
                RepeatPasswordValidationMessage.Content = string.Empty;
                RepeatPasswordValidationMessage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
