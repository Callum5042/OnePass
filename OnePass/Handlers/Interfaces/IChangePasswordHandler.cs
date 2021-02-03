using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface IChangePasswordHandler
    {
        Task<string> ChangePassword(string oldPassword, string newPassword);
    }
}