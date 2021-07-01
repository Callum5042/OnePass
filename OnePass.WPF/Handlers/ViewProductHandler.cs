using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IViewProductHandler))]
    public class ViewProductHandler : IViewProductHandler
    {
        private readonly IEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;

        public ViewProductHandler(IEncryptor encryptor, OnePassRepository onePassRepository)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return ReadJsonAsync();
        }

        private async Task<List<Product>> ReadJsonAsync()
        {
            var filename = _onePassRepository.Filename;
            var password = _onePassRepository.MasterPassword;

            var json = await _encryptor.DecryptAsync(filename, password);

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products.ToList();
        }
    }
}
