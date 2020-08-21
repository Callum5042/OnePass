using OnePass.Infrastructure;
using OnePass.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IAddProductHandler))]
    public class AddProductHandler : IAddProductHandler
    {
        public AddProductHandler()
        {
        }

        public async Task AddProduct(Product product)
        {
            var products = await ReadJsonAsync();
            products.Add(product);

            await SaveJsonAsync(new ProductRoot()
            {
                Products = products
            });
        }

        private async Task<IList<Product>> ReadJsonAsync()
        {
            // Read data
            using var file = File.OpenRead(@"data.json");
            using var reader = new StreamReader(file);
            var json = await reader.ReadToEndAsync();

            var products = JsonSerializer.Deserialize<ProductRoot>(json);
            return products.Products.ToList();
        }

        private async Task SaveJsonAsync(ProductRoot root)
        {
            var json = JsonSerializer.Serialize(root);

            using var file = File.OpenWrite(@"data.json");
            using var writer = new StreamWriter(file);
            await writer.WriteAsync(json);
        }
    }
}
