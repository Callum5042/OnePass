using OnePass.Services;
using System.IO;
using System.Threading.Tasks;

namespace OnePass.WPF.Tests
{
    public class MockEncryptor : IFileEncryptor
    {
        public async Task DecryptAsync(Stream input, Stream output, string password)
        {
            await input.CopyToAsync(output);
        }

        public async Task EncryptAsync(Stream input, Stream output, string password)
        {
            await input.CopyToAsync(output);
        }
    }
}
