using OnePass.Infrastructure;
using OnePass.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for LoginPage2.xaml
    /// </summary>
    [Inject]
    public partial class LoginPage : Page
    {
        private readonly ISettingsMonitor _settingsMonitor;

        public LoginPage(ISettingsMonitor settingsMonitor)
        {
            InitializeComponent();
            _settingsMonitor = settingsMonitor;

            Username.Focus();
        }

        private void OnClick_Login(object sender, RoutedEventArgs e)
        {
            var usernameValid = ValidateUsernameSyntax();
            var passwordValid = ValidatePasswordSyntax();

            if (usernameValid && passwordValid)
            {
                var app = Application.Current as App;

                var window = app.GetService<MainWindow>();
                window.Show();

                var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                loginWindow.Close();
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
