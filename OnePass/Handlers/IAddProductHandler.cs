using OnePass.Services;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IAddProductHandler
    {
        Task AddProduct(Product product);
    }
}