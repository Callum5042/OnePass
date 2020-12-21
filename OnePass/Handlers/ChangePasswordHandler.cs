using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IChangePasswordHandler))]
    public class ChangePasswordHandler : IChangePasswordHandler
    {
        private readonly IEncryptor _encryptor;
        private readonly ISettingsMonitor _settingsMonitor;

        public ChangePasswordHandler(IEncryptor encryptor, ISettingsMonitor settingsMonitor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
        }

        public async Task<string> ChangePassword(string oldPassword, string newPassword)
        {
            if (_settingsMonitor.Current.MasterPassword == oldPassword)
            {
                _settingsMonitor.Current.MasterPassword = newPassword;

                // Hash new password
                await HashPassword(newPassword);

                // Decrypt and Encrypt data with new master password
                var json = await _encryptor.DecryptAsync(_settingsMonitor.Current.FileName, oldPassword);
                await _encryptor.EncryptAsync(_settingsMonitor.Current.FileName, newPassword, json);

                return "Password has been changed";
            }
            else
            {
                return "Current password is invalid";
            }
        }

        private async Task HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var passwordBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + _settingsMonitor.Current.Salt));
            var hash = Encoding.UTF8.GetString(passwordBytes);

            _settingsMonitor.Current.Hash = hash;
            await _settingsMonitor.SaveAsync();
        }
    }
}
