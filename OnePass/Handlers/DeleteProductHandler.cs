using OnePass.Infrastructure;
using OnePass.Services.DataAccess;
using System;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IDeleteProductHandler))]
    public class DeleteProductHandler : IDeleteProductHandler
    {
        private readonly OnePassContext _context;

        public DeleteProductHandler(OnePassContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
