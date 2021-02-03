using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface ILoginHandler
    {
        Task<LoginResult> LoginAsync(string username, string password);
    }
}