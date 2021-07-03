using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IRegisterAccountHandler))]
    public class RegisterAccountHandler : IRegisterAccountHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IHasher _hasher;
        private readonly IFileEncryptor _encryptor;

        public string Filename { get; set; } = @"usermapping.json";

        public RegisterAccountHandler(IFileSystem fileSystem, IHasher hasher, IFileEncryptor encryptor)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        public async Task<RegisterAccountResult> RegisterAccountAsync(string username, string password)
        {
            var accountRoot = new AccountRoot();
            if (_fileSystem.File.Exists(Filename))
            {
                accountRoot = await ConvertJsonAsync();

                // Check if username already exists
                if (accountRoot.Accounts.Any(x => x.Username.Equals(username)))
                {
                    return RegisterAccountResult.UsernameAlreadyExists;
                }
            }

            // Create account
            var salt = Guid.NewGuid().ToString();
            var hash = _hasher.ComputeHashToString(password + salt);

            var account = new Account()
            {
                Username = username,
                Password = hash,
                Salt = salt,
                Filename = $"{username}.bin"
            };

            accountRoot.Accounts.Add(account);
            await SaveJsonAsync(accountRoot);

            var json = JsonSerializer.Serialize(new ProductRoot()
            {
                Products = new List<Product>()
            });

            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);
            using var file = _fileSystem.File.OpenWrite(account.Filename);
            await _encryptor.EncryptAsync(memory, file, password);

            return RegisterAccountResult.Success;
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
            using var file = _fileSystem.File.OpenWrite(Filename);
            using var writer = new StreamWriter(file);

            var json = JsonSerializer.Serialize(accountRoot);
            await writer.WriteLineAsync(json);
        }
    }
}
