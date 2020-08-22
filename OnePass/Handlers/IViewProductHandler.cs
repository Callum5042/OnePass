using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IViewProductHandler
    {
        Task<List<Product>> GetAllProductsAsync();
    }
}