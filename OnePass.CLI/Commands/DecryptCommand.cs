using OnePass.Services;
using System;
using System.IO.Abstractions;

namespace OnePass.CLI.Commands
{
    public class DecryptCommand : ICommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _fileEncryptor;

        public DecryptCommand(IFileSystem fileSystem, IFileEncryptor fileEncryptor)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _fileEncryptor = fileEncryptor ?? throw new ArgumentNullException(nameof(fileEncryptor));
        }

        public CommandType CommandType => CommandType.Decrypt;

        public void Execute(Arguments arguments)
        {
            using var file = _fileSystem.File.OpenRead(arguments.File);
            using var stream = _fileEncryptor.Decrypt(file, "super");

            using var outputfile = _fileSystem.File.Create("decrypted_file.json");
            stream.CopyTo(outputfile);
        }
    }
}
