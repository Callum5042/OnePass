using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface IViewProductHandler
    {
        Task<List<Product>> GetAllProductsAsync();
    }
}