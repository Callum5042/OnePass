using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface ILoginHandler
    {
        Task<LoginResult> Login(string username, string password);
    }
}