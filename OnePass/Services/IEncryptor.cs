using System.Threading.Tasks;

namespace OnePass.Services.Interfaces
{
    public interface IEncryptor
    {
        Task<string> DecryptAsync(string file, string password);

        Task EncryptAsync(string file, string password, string data);
    }
}