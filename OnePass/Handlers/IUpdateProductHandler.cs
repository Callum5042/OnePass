using OnePass.Services;
using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IUpdateProductHandler
    {
        Task UpdateAsync(int productId, Product product);
    }
}