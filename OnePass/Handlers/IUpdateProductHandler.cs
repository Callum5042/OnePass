using OnePass.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IUpdateProductHandler
    {
        Task<IEnumerable<Product>> UpdateAsync(Product model);
    }
}