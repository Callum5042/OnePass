using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IRegisterAccountHandler))]
    public class RegisterAccountHandler : IRegisterAccountHandler
    {
        const string _filename = @"usermapping.json";
        private readonly IHasher _hasher;

        public RegisterAccountHandler(IHasher hasher)
        {
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<RegisterAccountResult> RegisterAccountAsync(string username, string password)
        {
            var accountRoot = new AccountRoot();
            if (File.Exists(_filename))
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
                Password = password,
                Salt = salt,
                Filename = $"{username}.bin"
            };

            accountRoot.Accounts.Add(account);
            await SaveJsonAsync(accountRoot);

            return RegisterAccountResult.Success;
        }

        private static async Task<AccountRoot> ConvertJsonAsync()
        {
            using var fileRead = File.OpenRead(_filename);
            using var reader = new StreamReader(fileRead);

            var readJson = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<AccountRoot>(readJson);
        }

        private static async Task SaveJsonAsync(AccountRoot accountRoot)
        {
            using var file = File.OpenWrite(_filename);
            using var writer = new StreamWriter(file);

            var json = JsonSerializer.Serialize(accountRoot);
            await writer.WriteLineAsync(json);
        }
    }
}
