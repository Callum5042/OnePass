using System.IO;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public interface IFileEncryptor
    {
        Task EncryptAsync(Stream input, Stream output, string password);

        Task DecryptAsync(Stream input, Stream output, string password);
    }
}