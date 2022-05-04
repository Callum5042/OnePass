﻿using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow2.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string _passwordEye = "";
        private const string _passwordEyeBlocked = "";

        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            SetCapsLockWarning();
        }

        private void SetCapsLockWarning()
        {
            if ((Keyboard.GetKeyStates(Key.CapsLock) & KeyStates.Toggled) == KeyStates.Toggled)
            {
                CapsLockWarningLabel.Visibility = Visibility.Visible;
            }
            else
            {
                CapsLockWarningLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetCapsLockWarning();

            // Login
            LoginUsernameTextbox.Focus();
        }

        private void TextboxPassword_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Disable copy & pasting on the password box
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void OnClickTogglePasswordField(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var textbox = LogicalTreeHelper.GetChildren(button.Parent).OfType<TextBox>().First();
            if (button.Content as string == _passwordEyeBlocked)
            {
                button.Content = _passwordEye;
                textbox.FontFamily = new FontFamily("Segoe UI");
            }
            else
            {
                button.Content = _passwordEyeBlocked;
                textbox.FontFamily = App.Current.TryFindResource("PasswordFonts") as FontFamily;
            }
        }

        private void OnClickLoginButton(object sender, RoutedEventArgs e)
        {
            if (ValidateLoginModel())
            {
                MessageBox.Show("Is valid");
            }
        }

        private void OnClickRegisterButton(object sender, RoutedEventArgs e)
        {
            LoginStackPanel.Visibility = Visibility.Collapsed;
            RegisterStackPanel.Visibility = Visibility.Visible;
        }

        private void OnClickBackButton(object sender, RoutedEventArgs e)
        {
            LoginStackPanel.Visibility = Visibility.Visible;
            RegisterStackPanel.Visibility = Visibility.Collapsed;
        }

        private void OnClickCreateAccountButton(object sender, RoutedEventArgs e)
        {
            if (ValidateCreateAccount())
            {
                MessageBox.Show("Create Account");
            }
        }

        private bool ValidateCreateAccount()
        {
            var username = RegisterUsernameTextbox.Text;
            var password = RegisterPasswordTextbox.Text;
            var repeatPassword = RegisterPasswordRepeatTextbox.Text;
            var isValid = true;

            // Validate repeat password
            if (string.IsNullOrEmpty(repeatPassword))
            {
                RegisterRepeatPasswordValidationLabel.Content = "Password is required.";
                RegisterRepeatPasswordValidationLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                if (password != repeatPassword)
                {
                    RegisterRepeatPasswordValidationLabel.Content = "Password must match Repeat Password.";
                    RegisterRepeatPasswordValidationLabel.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    RegisterRepeatPasswordValidationLabel.Visibility = Visibility.Collapsed;
                }
            }

            // Validate password
            if (string.IsNullOrEmpty(password))
            {
                RegisterPasswordValidationLabel.Content = "Password is required.";
                RegisterPasswordValidationLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                if (password.Length < 10)
                {
                    RegisterPasswordValidationLabel.Content = "Password must be at least 10 characters.";
                    RegisterPasswordValidationLabel.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    RegisterPasswordValidationLabel.Visibility = Visibility.Collapsed;
                }
            }

            // Validate username
            if (string.IsNullOrEmpty(username))
            {
                RegisterUsernameValidationLabel.Content = "Username is required.";
                RegisterUsernameValidationLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                RegisterUsernameValidationLabel.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }

        private bool ValidateLoginModel()
        {
            var username = LoginUsernameTextbox.Text;
            var password = LoginPasswordTextbox.Text;
            var isValid = true;

            // Validate password
            if (string.IsNullOrEmpty(password))
            {
                LoginPasswordValidationLabel.Content = "Password is required.";
                LoginPasswordValidationLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                if (password.Length < 10)
                {
                    LoginPasswordValidationLabel.Content = "Password must be at least 10 characters.";
                    LoginPasswordValidationLabel.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    LoginPasswordValidationLabel.Visibility = Visibility.Collapsed;
                }
            }

            // Validate username
            if (string.IsNullOrEmpty(username))
            {
                LoginUsernameValidationLabel.Content = "Username is required.";
                LoginUsernameValidationLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                LoginUsernameValidationLabel.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }
    }
}
