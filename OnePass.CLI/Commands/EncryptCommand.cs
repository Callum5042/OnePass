using OnePass.Services;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace OnePass.CLI.Commands
{
    public class EncryptCommand : ICommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _fileEncryptor;

        public EncryptCommand(IFileSystem fileSystem, IFileEncryptor fileEncryptor)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _fileEncryptor = fileEncryptor ?? throw new ArgumentNullException(nameof(fileEncryptor));
        }

        public CommandType CommandType => CommandType.Encrypt;

        public async Task ExecuteAsync(Arguments arguments)
        {
            using var file = _fileSystem.File.OpenRead(arguments.File);
            using var output = _fileSystem.File.Create("encrypted.txt");
            await _fileEncryptor.EncryptAsync(file, output, "super");
        }
    }
}
