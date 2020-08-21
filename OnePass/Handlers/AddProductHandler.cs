using OnePass.Infrastructure;
using OnePass.Services;
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
        private readonly ISettingsMonitor _settingsMonitor;

        public AddProductHandler(IEncryptor encryptor, ISettingsMonitor settingsMonitor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _settingsMonitor = settingsMonitor;
        }

        public async Task AddProduct(Product model)
        {
            var products = await ReadJsonAsync();
            products.Add(model);

            for (int i = 0; i < products.Count; i++)
            {
                products[i].Id = i;
            }

            await SaveJsonAsync(new ProductRoot()
            {
                Products = products
            });
        }

        private async Task<IList<Product>> ReadJsonAsync()
        {
            var json = await _encryptor.DecryptAsync(_settingsMonitor.Current.FileName, _settingsMonitor.Current.MasterPassword);

            var products = JsonSerializer.Deserialize<ProductRoot>(json);
            return products.Products.ToList();
        }

        private async Task SaveJsonAsync(ProductRoot root)
        {
            var json = JsonSerializer.Serialize(root);
            await _encryptor.EncryptAsync(_settingsMonitor.Current.FileName, _settingsMonitor.Current.MasterPassword, json);
        }
    }
}
