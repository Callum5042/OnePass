using OnePass.WPF.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow2.xaml
    /// </summary>
    public partial class LoginWindow2 : Window
    {
        public LoginWindow2()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private bool _showPassword = false;
        private const string _passwordEyeBlocked = "";
        private const string _passwordEye = "";

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

        private void TextboxPassword_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Disable copy & pasting on the password box
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetCapsLockWarning();
            TextboxUsername.Focus();

            // Load options
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, @"OnePass", "options.json");
            if (File.Exists(path))
            {
                using var file = File.OpenRead(path);
                var options = await JsonSerializer.DeserializeAsync<AppOptions>(file);

                // Set remember username
                if (!string.IsNullOrWhiteSpace(options.RememberUsername))
                {
                    throw new NotImplementedException("Load option file");
                }
            }
        }
    }
}
