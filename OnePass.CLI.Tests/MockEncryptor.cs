using OnePass.Services;
using System.IO;

namespace OnePass.CLI.Tests
{
    public class MockEncryptor : IFileEncryptor
    {
        public void Encrypt(Stream input, Stream output, string password)
        {
            input.CopyTo(output);
        }

        public void Decrypt(Stream input, Stream output, string password)
        {
            input.CopyTo(output);
        }
    }
}
