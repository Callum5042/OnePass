using System.IO;

namespace OnePass.Services
{
    public interface IFileEncryptor
    {
        Stream Decrypt(Stream stream, string password);
    }
}