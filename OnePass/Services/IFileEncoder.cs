using OnePass.Models;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public interface IFileEncoder
    {
        Task<RootAccount> LoadAsync(string username, string password, string path = null);

        Task SaveAsync(string username, string password, RootAccount rootAccount, string path = null);

        bool Verify(string username, string password, string path = null);
    }
}
