using OnePass.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void OnActivated(object sender, EventArgs e)
        {
            SetCapsLockWarning();
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
    }
}
