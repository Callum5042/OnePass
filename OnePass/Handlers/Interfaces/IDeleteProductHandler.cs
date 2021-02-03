using OnePass.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface IDeleteProductHandler
    {
        Task<IEnumerable<Product>> DeleteProductAsync(Product model);
    }
}