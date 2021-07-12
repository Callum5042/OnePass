using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.IO;
using System.IO.Abstractions;
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
    }
}
