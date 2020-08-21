using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Services.DataAccess;
using System;
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
            var dbProduct = await _context.Products.FindAsync(productId);
            if (dbProduct == null)
            {
                throw new InvalidOperationException();
            }

            dbProduct.Name = product.Name;
            dbProduct.Login = product.Login;
            dbProduct.Password = product.Password;

            await _context.SaveChangesAsync();
        }
    }
}
