using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IUpdateProductHandler))]
    public class UpdateProductHandler : IUpdateProductHandler
    {
        private readonly OnePassContext _context;

        public UpdateProductHandler(OnePassContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task UpdateAsync(int productId, Product product)
        {
            //var dbProduct = await _context.Products.FindAsync(productId);
            //if (dbProduct == null)
            //{
            //    throw new InvalidOperationException();
            //}

            //dbProduct.Name = product.Name;
            //dbProduct.Login = product.Login;
            //dbProduct.Password = product.Password;

            //await _context.SaveChangesAsync();


            var products = await ReadJsonAsync();

            var toUpdate = products.First(x => x.Name == x.Name);
            toUpdate = product;

            //var x = products.Remove(toRemove);

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

            using var writer = new StreamWriter(@"data.json", false);
            await writer.WriteAsync(json);
        }
    }
}
