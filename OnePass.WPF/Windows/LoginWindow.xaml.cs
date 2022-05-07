using OnePass.Models;
using OnePass.WPF.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetCapsLockWarning();

            // Check if selected
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, @"OnePass", "options.json");
            if (File.Exists(path))
            {
                using var file = File.OpenRead(path);
                var options = JsonSerializer.Deserialize<AppOptions>(file);

                if (string.IsNullOrEmpty(options.RememberUsername))
                {
                    LoginUsernameTextbox.Focus();
                }
                else
                {
                    LoginUsernameTextbox.Text = options.RememberUsername;
                    LoginPasswordTextbox.Focus();
                    RememberMeCheckbox.IsChecked = true;
                }
            }
            else
            {
                LoginUsernameTextbox.Focus();
            }
        }

        private static void SaveOptions(AppOptions appOptions)
        {
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory(Path.Combine(appdata, "OnePass"));
                var path = Path.Combine(appdata, @"OnePass", "options.json");
                using var file = File.Create(path);
                JsonSerializer.Serialize(file, appOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Waring", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void OnClickLoginButton(object sender, RoutedEventArgs e)
        {
            if (LoginStackPanel.DataContext is LoginModel model)
            {
                var (valid, fileNotFound, invalidPassword) = model.IsValid();
                if (valid)
                {
                    // Save options
                    // if (RememberMeCheckbox.IsChecked == true)
                    // {
                    //     SaveOptions(new AppOptions()
                    //     {
                    //         RememberUsername = LoginUsernameTextbox.Text,
                    //     });
                    // }
                    // else
                    // {
                    //     SaveOptions(new AppOptions()
                    //     {
                    //         RememberUsername = string.Empty,
                    //     });
                    // }

                    // Set login details
                    App.Current.Username = model.Username;
                    App.Current.Password = model.Password;

                    // Change window
                    var contentWindow = new ContentWindow();
                    contentWindow.Show();
                    Close();
                }
                else if (fileNotFound)
                {
                    var bindingExpression = BindingOperations.GetBindingExpression(LoginUsernameTextbox, TextBox.TextProperty);
                    var bindingExpressionBase = BindingOperations.GetBindingExpressionBase(LoginUsernameTextbox, TextBox.TextProperty);
                    var validationError = new ValidationError(new ExceptionValidationRule(), bindingExpression);

                    Validation.MarkInvalid(bindingExpressionBase, validationError);
                }
                else if (invalidPassword)
                {
                    var bindingExpression = BindingOperations.GetBindingExpression(LoginPasswordTextbox, TextBox.TextProperty);
                    var bindingExpressionBase = BindingOperations.GetBindingExpressionBase(LoginPasswordTextbox, TextBox.TextProperty);
                    var validationError = new ValidationError(new ExceptionValidationRule(), bindingExpression);

                    Validation.MarkInvalid(bindingExpressionBase, validationError);
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
            //if (ValidateCreateAccount())
            //{
            //    // Check if file already exists
            //    if (File.Exists($"{RegisterUsernameTextbox.Text}.bin"))
            //    {
            //        RegisterUsernameValidationLabel.Visibility= Visibility.Visible;
            //        RegisterUsernameValidationLabel.Content = $"{RegisterUsernameTextbox.Text}.bin already exists";
            //        return;
            //    }

            //    // Create new file
            //    CreateFile();

            //    // Set login details
            //    App.Current.Username = RegisterUsernameTextbox.Text;
            //    App.Current.Password = RegisterPasswordTextbox.Text;

            //    // Change window
            //    var contentWindow = new ContentWindow();
            //    contentWindow.Show();
            //    Close();
            //}
        }

        private void CreateFile()
        {
            var fileSignature = ".ONEPASS";
            var fileVersion = 1;

            // Generate salt
            var salt = RandomNumberGenerator.GetBytes(8);

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(RegisterPasswordTextbox.Text, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);

            using var file = File.Create($"{RegisterUsernameTextbox.Text}.bin");
            using var writer = new BinaryWriter(file);

            // Write signature
            writer.Write(Encoding.UTF8.GetBytes(fileSignature));

            // Write version
            writer.Write(fileVersion);

            // Write password hash
            using (var sha = SHA512.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(RegisterPasswordTextbox.Text);
                var bytes = passwordBytes.Concat(salt).ToArray();
                var passwordHash = sha.ComputeHash(bytes);

                writer.Write(passwordHash.Length);
                writer.Write(passwordHash);
            }

            // Write salt
            writer.Write(salt.Length);
            writer.Write(salt);

            // Write IV
            writer.Write(aes.IV.Length);
            writer.Write(aes.IV);

            // Encrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var cryptoWriter = new StreamWriter(cryptoStream);

            var accountRoot = new RootAccount();
            var content = JsonSerializer.Serialize(accountRoot);
            cryptoWriter.Write(content);
        }

        //private bool ValidateCreateAccount()
        //{
        //    var username = RegisterUsernameTextbox.Text;
        //    var password = RegisterPasswordTextbox.Text;
        //    var repeatPassword = RegisterPasswordRepeatTextbox.Text;
        //    var isValid = true;

        //    // Validate repeat password
        //    if (string.IsNullOrEmpty(repeatPassword))
        //    {
        //        RegisterRepeatPasswordValidationLabel.Content = "Password is required.";
        //        RegisterRepeatPasswordValidationLabel.Visibility = Visibility.Visible;
        //        isValid = false;
        //    }
        //    else
        //    {
        //        if (password != repeatPassword)
        //        {
        //            RegisterRepeatPasswordValidationLabel.Content = "Password must match Repeat Password.";
        //            RegisterRepeatPasswordValidationLabel.Visibility = Visibility.Visible;
        //            isValid = false;
        //        }
        //        else
        //        {
        //            RegisterRepeatPasswordValidationLabel.Visibility = Visibility.Collapsed;
        //        }
        //    }

        //    // Validate password
        //    if (string.IsNullOrEmpty(password))
        //    {
        //        RegisterPasswordValidationLabel.Content = "Password is required.";
        //        RegisterPasswordValidationLabel.Visibility = Visibility.Visible;
        //        isValid = false;
        //    }
        //    else
        //    {
        //        if (password.Length < 10)
        //        {
        //            RegisterPasswordValidationLabel.Content = "Password must be at least 10 characters.";
        //            RegisterPasswordValidationLabel.Visibility = Visibility.Visible;
        //            isValid = false;
        //        }
        //        else
        //        {
        //            RegisterPasswordValidationLabel.Visibility = Visibility.Collapsed;
        //        }
        //    }

        //    // Validate username
        //    if (string.IsNullOrEmpty(username))
        //    {
        //        RegisterUsernameValidationLabel.Content = "Username is required.";
        //        RegisterUsernameValidationLabel.Visibility = Visibility.Visible;
        //        isValid = false;
        //    }
        //    else
        //    {
        //        RegisterUsernameValidationLabel.Visibility = Visibility.Collapsed;
        //    }

        //    return isValid;
        //}
    }
}
