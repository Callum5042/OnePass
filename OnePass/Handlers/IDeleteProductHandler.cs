using OnePass.Services;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IDeleteProductHandler
    {
        Task DeleteProductAsync(Product product);
    }
}