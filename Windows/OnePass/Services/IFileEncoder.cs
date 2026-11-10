using OnePass.Models;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public interface IFileEncoder
    {
        /// <summary>
        /// Decrypts a vault. Throws <see cref="InvalidPasswordException"/> when the password is wrong.
        /// </summary>
        Task<OnePassData> LoadAsync(string username, string password, string path = null);

        Task SaveAsync(string username, string password, OnePassData rootAccount, string path = null);

        /// <summary>
        /// True when the file uses an older format that the next save will replace.
        /// </summary>
        bool NeedsUpgrade(string username, string path = null);
    }
}
