using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IChangePasswordHandler))]
    public class ChangePasswordHandler : IChangePasswordHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;
        private readonly IHasher _hasher;

        public string Filename { get; set; } = @"usermapping.json";

        public ChangePasswordHandler(IFileSystem fileSystem, IFileEncryptor encryptor, OnePassRepository onePassRepository, IHasher hasher)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<bool> ChangePassword(string oldPassword, string newPassword)
        {
            if (_onePassRepository.MasterPassword == oldPassword)
            {
                _onePassRepository.MasterPassword = newPassword;

                // Hash new password
                await HashPassword(newPassword);

                // Decrypt file
                using var file = _fileSystem.File.OpenRead(_onePassRepository.Filename);
                using var memory = new MemoryStream();
                await _encryptor.DecryptAsync(file, memory, oldPassword);

                memory.Seek(0, SeekOrigin.Begin);

                // Encrypt file with new password
                using var output = _fileSystem.File.OpenWrite(_onePassRepository.Filename);
                output.SetLength(0);
                await _encryptor.EncryptAsync(memory, output, newPassword);

                return true;
            }

            return false;
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
            using var fileRead = _fileSystem.File.OpenRead(Filename);
            using var reader = new StreamReader(fileRead);

            var readJson = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<AccountRoot>(readJson);
        }

        private async Task SaveJsonAsync(AccountRoot accountRoot)
        {
            var json = JsonSerializer.Serialize(accountRoot);
            await _fileSystem.File.WriteAllTextAsync(Filename, json);
        }
    }
}
