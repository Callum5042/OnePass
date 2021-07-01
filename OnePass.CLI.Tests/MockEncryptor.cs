using OnePass.Services;
using System.IO;

namespace OnePass.CLI.Tests
{
    public class MockEncryptor : IFileEncryptor
    {
        public Stream Decrypt(Stream stream, string password)
        {
            return stream;
        }
    }
}
