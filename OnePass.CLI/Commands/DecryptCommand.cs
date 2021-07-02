﻿using OnePass.Services;
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
            using var output = _fileSystem.File.Create("encrypted.txt");
            _fileEncryptor.Decrypt(file, output, "super");
        }
    }
}
