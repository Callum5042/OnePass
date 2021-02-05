using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IChangePasswordHandler))]
    public class ChangePasswordHandler : IChangePasswordHandler
    {
        private readonly IEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;
        private readonly IHasher _hasher;

        public string Filename { get; set; } = @"usermapping.json";

        public ChangePasswordHandler(IEncryptor encryptor, OnePassRepository onePassRepository, IHasher hasher)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<string> ChangePassword(string oldPassword, string newPassword)
        {
            if (_onePassRepository.MasterPassword == oldPassword)
            {
                _onePassRepository.MasterPassword = newPassword;

                // Hash new password
                await HashPassword(newPassword);

                // Decrypt and Encrypt data with new master password
                var json = await _encryptor.DecryptAsync(_onePassRepository.Filename, oldPassword);
                await _encryptor.EncryptAsync(_onePassRepository.Filename, newPassword, json);

                return "Password has been changed";
            }
            else
            {
                return "Current password is invalid";
            }
        }

        private async Task HashPassword(string password)
        {
            var accountRoot = await ConvertJsonAsync();
            var account = accountRoot.Accounts.FirstOrDefault(x => x.Username.Equals(_onePassRepository.Username));
            if (account != null)
            {
                var hash = _hasher.ComputeHashToString(password + account.Salt);
                account.Password = hash;

                await SaveJsonAsync(accountRoot);
            }
        }

        private async Task<AccountRoot> ConvertJsonAsync()
        {
            using var fileRead = File.OpenRead(Filename);
            using var reader = new StreamReader(fileRead);

            var readJson = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<AccountRoot>(readJson);
        }

        private async Task SaveJsonAsync(AccountRoot accountRoot)
        {
            var json = JsonSerializer.Serialize(accountRoot);
            await File.WriteAllTextAsync(Filename, json);
        }
    }
}
