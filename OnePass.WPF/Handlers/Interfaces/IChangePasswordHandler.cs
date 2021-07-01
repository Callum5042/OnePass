using System.Threading.Tasks;

namespace OnePass.Handlers.Interfaces
{
    public interface IChangePasswordHandler
    {
        Task<bool> ChangePassword(string oldPassword, string newPassword);
    }
}