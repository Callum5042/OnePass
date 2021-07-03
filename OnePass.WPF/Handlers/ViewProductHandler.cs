using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IViewProductHandler))]
    public class ViewProductHandler : IViewProductHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;

        public ViewProductHandler(IFileSystem fileSystem, IFileEncryptor encryptor, OnePassRepository onePassRepository)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var filename = _onePassRepository.Filename;
            var password = _onePassRepository.MasterPassword;

            // Decrypt file
            using var input = _fileSystem.File.OpenRead(filename);
            using var output = new MemoryStream();
            await _encryptor.DecryptAsync(input, output, password);

            // Read content
            output.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(output);
            var json = reader.ReadToEnd();

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products.ToList();
        }
    }
}
