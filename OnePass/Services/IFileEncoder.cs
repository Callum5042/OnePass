using OnePass.Models;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public interface IFileEncoder
    {
        Task<RootAccount> LoadAsync(string username, string password);

        Task SaveAsync(string username, string password, RootAccount rootAccount);

        bool Verify(string username, string password);
    }
}
