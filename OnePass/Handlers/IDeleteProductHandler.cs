using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IDeleteProductHandler
    {
        Task DeleteProductAsync(int productId);
    }
}