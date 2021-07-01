using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface IRegisterAccountHandler
    {
        Task<RegisterAccountResult> RegisterAccountAsync(string username, string password);
    }
}