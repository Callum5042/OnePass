using System.IO;

namespace OnePass.Services
{
    public interface IFileEncryptor
    {
        void Encrypt(Stream input, Stream output, string password);

        void Decrypt(Stream input, Stream output, string password);
    }
}