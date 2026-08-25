using OnePass.WPF.Models;
using OnePass.WPF.Services;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;

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
            DataContext = App.Current.GetService<LoginModel>();
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
                    LoginFileBrowseButton.Focus();
                }
            }
        }

        private void OnFilenameMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            SelectVaultFile();
        }

        private void OnClickSelectFile(object sender, RoutedEventArgs e)
        {
            SelectVaultFile();
        }

        private void OnRegisterFilenameMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            CreateVaultFile();
        }

        private void OnRegisterClickSelectFile(object sender, RoutedEventArgs e)
        {
            CreateVaultFile();
        }

        private void CreateVaultFile()
        {
            if (DataContext is not LoginModel model)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Create OnePass vault",
                Filter = "OnePass vault (*.bin)|*.bin|OnePass vault (*.onepass)|*.onepass|All files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = false,
                CheckPathExists = false,
                Multiselect = false,
            };

            if (!string.IsNullOrWhiteSpace(model.Login.FilePath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(model.Login.FilePath);
                dialog.FileName = Path.GetFileName(model.Login.FilePath);
            }

            if (dialog.ShowDialog(this) == true)
            {
                model.Register.FilePath = Path.GetFullPath(dialog.FileName);
                RegisterPasswordTextbox.Focus();
            }
        }

        private void SelectVaultFile()
        {
            if (DataContext is not LoginModel model)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Choose OnePass vault",
                Filter = "OnePass vault (*.bin)|*.bin|OnePass vault (*.onepass)|*.onepass|All files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
            };

            if (!string.IsNullOrWhiteSpace(model.Login.FilePath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(model.Login.FilePath);
                dialog.FileName = Path.GetFileName(model.Login.FilePath);
            }

            if (dialog.ShowDialog(this) == true)
            {
                model.Login.FilePath = Path.GetFullPath(dialog.FileName);
                model.Login.FilePathValidation = null;
                LoginPasswordTextbox.Focus();
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
                if (!model.Login.IsValid())
                {
                    return;
                }

                SetLoginBusy(true);

                try
                {
                    var vault = await model.TryDecryptAsync();
                    if (vault is null)
                    {
                        return;
                    }

                    // Set login details
                    var data = App.Current.GetService<UserData>();
                    data.Username = Path.GetFileNameWithoutExtension(model.Login.FilePath);
                    data.FilePath = Path.GetFullPath(model.Login.FilePath);
                    data.Password = model.Login.Password;
                    data.InitialVaultData = vault;

                    // Save options only after the vault has been successfully decrypted
                    await model.SaveOptions();

                    // Change window
                    var contentWindow = new ContentWindow();
                    contentWindow.Show();
                    Close();
                }
                finally
                {
                    SetLoginBusy(false);
                }
            }
        }

        private void SetLoginBusy(bool isBusy)
        {
            LoginButton.IsEnabled = !isBusy;
            LoginButton.Content = isBusy ? "Decrypting..." : "Login";
            LoginFileBrowseButton.IsEnabled = !isBusy;
            LoginFilenameTextbox.IsEnabled = !isBusy;
            LoginPasswordTextbox.IsEnabled = !isBusy;
            RegisterButton.IsEnabled = !isBusy;
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

        private async void OnClickCreateAccountButton(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginModel model)
            {
                // Register
                if (model.Register.IsValid())
                {
                    // Create account
                    await model.CreateAccountAsync(model.Register.FilePath, model.Register.Password);

                    // Set login details
                    var data = App.Current.GetService<UserData>();
                    data.Username = model.Register.FileName;
                    data.FilePath = model.Register.FilePath;
                    data.Password = model.Register.Password;
                    data.InitialVaultData = null;

                    // Change window
                    var contentWindow = new ContentWindow();
                    contentWindow.Show();
                    Close();
                }
            }
        }
    }
}
