using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IUpdateProductHandler))]
    public class UpdateProductHandler : IUpdateProductHandler
    {
        private readonly IEncryptor _encryptor;
        private readonly ISettingsMonitor _settingsMonitor;

        public UpdateProductHandler(IEncryptor encryptor, ISettingsMonitor settingsMonitor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _settingsMonitor = settingsMonitor;
        }

        public async Task<IEnumerable<Product>> UpdateAsync(Product model)
        {
            var products = await ReadJsonAsync();

            var product = products.First(x => x.Id == model.Id);
            product.Name = model.Name;
            product.Login = model.Login;
            product.Password = model.Password;

            await SaveJsonAsync(new ProductRoot()
            {
                Products = products
            });

            return products;
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
