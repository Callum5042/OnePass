using OnePass.WPF.Models;
using System.Linq;
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

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginModel();
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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetCapsLockWarning();

            // Check if selected
            if (DataContext is LoginModel model)
            {
                await model.LoadOptions();
                if (model.Login.RememberMe)
                {
                    LoginPasswordTextbox.Focus();
                }
                else
                {
                    LoginUsernameTextbox.Focus();
                }
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

        private async void OnClickLoginButton(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginModel model)
            {
                if (model.Login.IsValid())
                {
                    // Save options
                    await model.SaveOptions();

                    // Set login details
                    App.Current.Username = model.Login.Username;
                    App.Current.Password = model.Login.Password;

                    // Change window
                    var contentWindow = new ContentWindow();
                    contentWindow.Show();
                    Close();
                }
            }
        }

        private void OnClickRegisterButton(object sender, RoutedEventArgs e)
        {
            LoginStackPanel.Visibility = Visibility.Collapsed;
            RegisterStackPanel.Visibility = Visibility.Visible;

            RegisterUsernameTextbox.Focus();
        }

        private void OnClickBackButton(object sender, RoutedEventArgs e)
        {
            LoginStackPanel.Visibility = Visibility.Visible;
            RegisterStackPanel.Visibility = Visibility.Collapsed;
        }

        private void OnClickCreateAccountButton(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginModel model)
            {
                if (model.Register.IsValid())
                {
                    // Create account
                    model.CreateAccount(model.Register.Username, model.Register.Password);

                    // Set login details
                    App.Current.Username = model.Register.Username;
                    App.Current.Password = model.Register.Password;

                    // Change window
                    var contentWindow = new ContentWindow();
                    contentWindow.Show();
                    Close();
                }
            }
        }
    }
}
