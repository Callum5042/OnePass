using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.WPF.Models;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OnePass.Handlers
{
    [Inject(typeof(ILoginHandler))]
    public class LoginHandler : ILoginHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly OnePassRepository _onePassRepository;
        private readonly IFileEncryptor _fileEncryptor;

        public LoginHandler(IFileSystem fileSystem, OnePassRepository onePassRepository, IFileEncryptor fileEncryptor)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
            _fileEncryptor = fileEncryptor ?? throw new ArgumentNullException(nameof(fileEncryptor));
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var filename = $"{username}.bin";

            // Check if file exists
            if (!_fileSystem.File.Exists(filename))
            {
                return LoginResult.InvalidUsername;
            }

            // Attempt to decrypt the file with given password
            try
            {
                using var file = _fileSystem.File.OpenRead(filename);
                using var memory = new MemoryStream();
                await _fileEncryptor.DecryptAsync(file, memory, password);

                _onePassRepository.Username = username;
                _onePassRepository.Filename = filename;
                _onePassRepository.MasterPassword = password;

                return LoginResult.Success;
            }
            catch (CryptographicException)
            {
                // This might not be the best way to detect an invalid key
                // https://crypto.stackexchange.com/questions/12178/why-should-i-use-authenticated-encryption-instead-of-just-encryption
                return LoginResult.InvalidPassword;
            }


            //if (_fileSystem.File.Exists(filename))
            //{
            //    var accountRoot = await ConvertJsonAsync(filename);

            //    // Check to see if account exists within the usermapping.json file
            //    var account = accountRoot.Accounts.FirstOrDefault(x => x.Username.Equals(username));
            //    if (account is null)
            //    {
            //        return LoginResult.InvalidUsername;
            //    }

            //    // Check if password is valid
            //    var saltedPassword = password + account.Salt;
            //    var hash = _hasher.ComputeHashToString(saltedPassword);
            //    if (hash == account.Password)
            //    {
            //        _onePassRepository.Username = account.Username;
            //        _onePassRepository.Filename = account.Filename;
            //        _onePassRepository.MasterPassword = password;

            //        return LoginResult.Success;
            //    }
            //    else
            //    {
            //        return LoginResult.InvalidPassword;
            //    }
            //}
            //else
            //{
            //    return LoginResult.InvalidUsername;
            //}
        }

        private async Task<AccountRoot> ConvertJsonAsync(string filename)
        {
            using var fileRead = _fileSystem.File.OpenRead(filename);
            using var reader = new StreamReader(fileRead);

            var readJson = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<AccountRoot>(readJson);
        }
    }
}
