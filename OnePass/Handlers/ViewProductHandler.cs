using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace OnePass.Handlers
{
    [Inject(typeof(IViewProductHandler))]
    public class ViewProductHandler : IViewProductHandler
    {
        private readonly IEncryptor _encryptor;

        public ViewProductHandler(IEncryptor encryptor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return ReadJsonAsync();
        }

        private async Task<List<Product>> ReadJsonAsync()
        {
            var app = (Application.Current as App);
            var json = await _encryptor.DecryptAsync(app.FileName, app.MasterPassword);

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products.ToList();
        }
    }
}
