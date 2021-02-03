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
    [Inject(typeof(ILoginHandler))]
    public class LoginHandler : ILoginHandler
    {
        private readonly IHasher _hasher;

        public LoginHandler(IHasher hasher)
        {
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            var filename = @"usermapping.json";
            if (File.Exists(filename))
            {
                var accountRoot = await ConvertJsonAsync(filename);

                // Check to see if account exists within the usermapping.json file
                var account = accountRoot.Accounts.FirstOrDefault(x => x.Username.Equals(username));
                if (account is null)
                {
                    return LoginResult.InvalidUsername;
                }

                // Check if password is valid
                var saltedPassword = password + account.Salt;
                var hash = _hasher.ComputeHashToString(saltedPassword);
                if (hash == account.Password)
                {
                    return LoginResult.Success;
                }
                else
                {
                    return LoginResult.InvalidPassword;
                }
            }
            else
            {
                return LoginResult.InvalidUsername;
            }
        }

        private static async Task<AccountRoot> ConvertJsonAsync(string filename)
        {
            using var fileRead = File.OpenRead(filename);
            using var reader = new StreamReader(fileRead);

            var readJson = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<AccountRoot>(readJson);
        }
    }
}
