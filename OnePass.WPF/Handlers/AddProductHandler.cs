﻿using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.WPF.Models;
using OnePass.Services;
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
    [Inject(typeof(IAddProductHandler))]
    public class AddProductHandler : IAddProductHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;

        public AddProductHandler(IFileSystem fileSystem, IFileEncryptor encryptor, OnePassRepository onePassRepository)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
        }

        public async Task<IEnumerable<Product>> AddProductAsync(Product model)
        {
            var products = await ReadJsonAsync();
            products.Add(model);

            for (int i = 0; i < products.Count; i++)
            {
                products[i].Id = i + 1;
            }

            await SaveJsonAsync(new ProductRoot() { Products = products });
            return products;
        }

        private async Task<IList<Product>> ReadJsonAsync()
        {
            using var input = _fileSystem.File.OpenRead(_onePassRepository.Filename);
            using var output = new MemoryStream();
            await _encryptor.DecryptAsync(input, output, _onePassRepository.MasterPassword);

            output.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(output);
            var json = await reader.ReadToEndAsync();

            var products = JsonSerializer.Deserialize<ProductRoot>(json);
            return products.Products.ToList();
        }

        private async Task SaveJsonAsync(ProductRoot root)
        {
            var json = JsonSerializer.Serialize(root);

            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);

            using var file = _fileSystem.File.OpenWrite(_onePassRepository.Filename);
            file.SetLength(0);
            await _encryptor.EncryptAsync(memory, file, _onePassRepository.MasterPassword);
        }
    }
}
