using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    [Inject]
    public partial class ChangePasswordWindow : Window
    {
        private readonly ISettingsMonitor _settingsMonitor;
        private readonly IEncryptor _encryptor;

        public ChangePasswordWindow(ISettingsMonitor settingsMonitor, IEncryptor encryptor)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;

            _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var oldPass = oldPassTextbox.Text;
            var newPass = newPassTextbox.Text;
            var repeatPass = repeatPassTextbox.Text;

            if (newPass != repeatPass)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            if (_settingsMonitor.Current.MasterPassword == oldPass)
            {
                _settingsMonitor.Current.MasterPassword = newPass;

                // Hash new password
                await HashPassword(newPass);

                // Decrypt and Encrypt data with new master password
                var json = await _encryptor.DecryptAsync(_settingsMonitor.Current.FileName, oldPass);
                await _encryptor.EncryptAsync(_settingsMonitor.Current.FileName, newPass, json);

                MessageBox.Show("Password has been changed");
            }
            else
            {
                MessageBox.Show("Current password is invalid");
            }
        }

        private async Task HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var entered_bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + _settingsMonitor.Current.Salt));
            var entered_str = Encoding.UTF8.GetString(entered_bytes);

            _settingsMonitor.Current.Hash = entered_str;
            await _settingsMonitor.SaveAsync();
        }
    }
}
