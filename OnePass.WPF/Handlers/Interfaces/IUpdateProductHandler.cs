using OnePass.WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface IUpdateProductHandler
    {
        Task<IEnumerable<Product>> UpdateAsync(Product model);
    }
}