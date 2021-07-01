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
    [Inject(typeof(IAddProductHandler))]
    public class AddProductHandler : IAddProductHandler
    {
        private readonly IEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;

        public AddProductHandler(IEncryptor encryptor, OnePassRepository onePassRepository)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
        }

        public async Task<IEnumerable<Product>> AddProduct(Product model)
        {
            var products = await ReadJsonAsync();
            products.Add(model);

            for (int i = 0; i < products.Count; i++)
            {
                products[i].Id = i + 1;
            }

            await SaveJsonAsync(new ProductRoot()
            {
                Products = products
            });

            return products;
        }

        private async Task<IList<Product>> ReadJsonAsync()
        {
            var json = await _encryptor.DecryptAsync(_onePassRepository.Filename, _onePassRepository.MasterPassword);

            var products = JsonSerializer.Deserialize<ProductRoot>(json);
            return products.Products.ToList();
        }

        private async Task SaveJsonAsync(ProductRoot root)
        {
            var json = JsonSerializer.Serialize(root);
            await _encryptor.EncryptAsync(_onePassRepository.Filename, _onePassRepository.MasterPassword, json);
        }
    }
}
