using OnePass.Services;
using OnePass.WPF.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OnePass.WPF.Controls
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        private const string _passwordEyeBlocked = "";
        private const string _passwordEye = "";
        private bool _showPassword = false;

        public LoginControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var loginModel = App.Current.GetService<LoginModel>();
                loginModel.LoginControl = this;

                DataContext = loginModel;
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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextboxUsername.Focus();

            // Load options
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var path = Path.Combine(appdata, @"OnePass", "options.json");
                if (File.Exists(path))
                {
                    using var file = File.OpenRead(path);
                    var options = await JsonSerializer.DeserializeAsync<AppOptions>(file);

                    // Set remember username
                    if (!string.IsNullOrWhiteSpace(options.RememberUsername))
                    {
                        TextboxUsername.Text = options.RememberUsername;
                        RememberMe.IsChecked = true;
                        TextboxPassword.Focus();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not load options", "Waring", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.Focus();
        }
    }
}
