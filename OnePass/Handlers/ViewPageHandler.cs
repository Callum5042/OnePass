using Microsoft.EntityFrameworkCore;
using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject]
    public class ViewPageHandler
    {
        private readonly OnePassContext _context;

        public ViewPageHandler(OnePassContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return _context.Products.ToListAsync();
        }

        private async Task<IEnumerable<Product>> ReadJsonAsync()
        {
            // Read data
            using var file = File.OpenRead(@"data.json");
            using var reader = new StreamReader(file);
            var json = await reader.ReadToEndAsync();

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products;
        }

        private void SeedData()
        {
            var product = new Product() { Name = "TestSite", Login = "Callum", Password = "password_123" };

            var root = new ProductRoot
            {
                Products = new List<Product>() { product }
            };

            var str = JsonSerializer.Serialize(root);

            using var file = File.OpenWrite("data.json");
            using var writer = new StreamWriter(file);
            writer.Write(str);
        }
    }
}
