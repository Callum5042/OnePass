using OnePass.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IAddProductHandler
    {
        Task<IEnumerable<Product>> AddProduct(Product product);
    }
}