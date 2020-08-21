using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace OnePass.Handlers
{
    [Inject(typeof(IUpdateProductHandler))]
    public class UpdateProductHandler : IUpdateProductHandler
    {
        private readonly IEncryptor _encryptor;

        public UpdateProductHandler(IEncryptor encryptor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        public async Task UpdateAsync(Product model)
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
        }

        private async Task<IList<Product>> ReadJsonAsync()
        {
            var app = (Application.Current as App);
            var json = await _encryptor.DecryptAsync(app.FileName, app.MasterPassword);

            var products = JsonSerializer.Deserialize<ProductRoot>(json);
            return products.Products.ToList();
        }

        private async Task SaveJsonAsync(ProductRoot root)
        {
            var json = JsonSerializer.Serialize(root);

            var app = (Application.Current as App);
            await _encryptor.EncryptAsync(app.FileName, app.MasterPassword, json);
        }
    }
}
