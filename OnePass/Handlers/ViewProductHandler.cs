using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Models;
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
        private readonly ISettingsMonitor _settingsMonitor;

        public ViewProductHandler(IEncryptor encryptor, ISettingsMonitor settingsMonitor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return ReadJsonAsync();
        }

        private async Task<List<Product>> ReadJsonAsync()
        {
            var json = await _encryptor.DecryptAsync(_settingsMonitor.Current.FileName, _settingsMonitor.Current.MasterPassword);

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products.ToList();
        }
    }
}
