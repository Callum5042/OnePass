using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Services.Interfaces;
using OnePass.WPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IRegisterAccountHandler))]
    public class RegisterAccountHandler : IRegisterAccountHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _encryptor;

        public string Filename { get; set; } = @"usermapping.json";

        public RegisterAccountHandler(IFileSystem fileSystem, IFileEncryptor encryptor)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        public async Task<RegisterAccountResult> RegisterAccountAsync(string username, string password)
        {
            var filename = $"{username}.bin";

            // Check if file already exists
            if (_fileSystem.File.Exists(filename))
            {
                return RegisterAccountResult.UsernameAlreadyExists;
            }

            // Create new empty encrypted file
            var json = JsonSerializer.Serialize(new List<Account>());
            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);
            using var file = _fileSystem.File.OpenWrite(filename);
            await _encryptor.EncryptAsync(memory, file, password);

            return RegisterAccountResult.Success;
        }
    }
}
