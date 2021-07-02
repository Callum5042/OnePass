using OnePass.Services;
using System.IO;
using System.Threading.Tasks;

namespace OnePass.CLI.Tests
{
    public class MockEncryptor : IFileEncryptor
    {
        public Task EncryptAsync(Stream input, Stream output, string password)
        {
            return input.CopyToAsync(output);
        }

        public Task DecryptAsync(Stream input, Stream output, string password)
        {
            return input.CopyToAsync(output);
        }
    }
}
