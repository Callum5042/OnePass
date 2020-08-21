using OnePass.Infrastructure;
using OnePass.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IViewProductHandler))]
    public class ViewProductHandler : IViewProductHandler
    {
        public ViewProductHandler()
        {
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return ReadJsonAsync();
        }

        private async Task<List<Product>> ReadJsonAsync()
        {
            // Read data
            using var file = File.OpenRead(@"data.json");
            using var reader = new StreamReader(file);
            var json = await reader.ReadToEndAsync();

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products.ToList();
        }
    }
}
