using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Services.DataAccess;
using System;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    [Inject(typeof(IAddProductHandler))]
    public class AddProductHandler : IAddProductHandler
    {
        private readonly OnePassContext _context;

        public AddProductHandler(OnePassContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task AddProduct(Product product)
        {
            _context.Products.Add(product);
            return _context.SaveChangesAsync();
        }
    }
}
