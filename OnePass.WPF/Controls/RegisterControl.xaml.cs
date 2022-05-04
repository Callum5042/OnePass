using OnePass.WPF.Models;
using OnePass.WPF.Windows;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OnePass.WPF.Controls
{
    /// <summary>
    /// Interaction logic for RegisterControl.xaml
    /// </summary>
    public partial class RegisterControl : UserControl, IWindowRegisterModel
    {
        private const string _passwordEyeBlocked = "";
        private const string _passwordEye = "";
        private bool _showPassword = false;
        private bool _showRepeatPassword = false;

        public RegisterControl()
        {
            InitializeComponent();
            DataContext = new RegisterModel(this);
        }

        public void ShowLoginControl()
        {
            //var window = App.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            //window.LoginControl.Visibility = Visibility.Visible;
            //window.RegisterControl.Visibility = Visibility.Collapsed;
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

            if (_showPassword)
            {
                button.Content = _passwordEye;
                _showPassword = false;

                TextboxPassword.FontFamily = App.Current.TryFindResource("PasswordFonts") as FontFamily;
            }
            else
            {
                button.Content = _passwordEyeBlocked;
                _showPassword = true;

                TextboxPassword.FontFamily = new FontFamily("Segoe UI");
            }
        }

        private void OnClickToggleRepeatPasswordField(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (_showRepeatPassword)
            {
                button.Content = _passwordEye;
                _showRepeatPassword = false;

                TextboxRepeatPassword.FontFamily = App.Current.TryFindResource("PasswordFonts") as FontFamily;
            }
            else
            {
                button.Content = _passwordEyeBlocked;
                _showRepeatPassword = true;

                TextboxRepeatPassword.FontFamily = new FontFamily("Segoe UI");
            }
        }
    }
}
