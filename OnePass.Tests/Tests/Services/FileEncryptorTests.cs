using OnePass.Services;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public class FileEncryptorTests
    {
        [Fact]
        public async Task EncryptAsync()
        {
            // Arrange
            using var input = new MemoryStream();
            using var writer = new StreamWriter(input);
            writer.WriteLine("testing");
            writer.Flush();
            input.Seek(0, SeekOrigin.Begin);

            // Act
            var encryptor = new FileEncryptor();

            using var output = new MemoryStream();
            await encryptor.EncryptAsync(input, output, "password");

            var content = output.ToArray();

            // Assert
            var encryptedContent = new byte[]
            {
                243, 103, 226, 11, 187, 51, 24, 71, 33, 69, 50, 185, 219, 116, 226, 60
            };

            Assert.NotEmpty(content);
            Assert.Equal(encryptedContent, content);
        }

        [Fact]
        public async Task DecryptAsync()
        {
            // Arrange
            var encryptedContent = new byte[]
            {
                243, 103, 226, 11, 187, 51, 24, 71, 33, 69, 50, 185, 219, 116, 226, 60
            };

            using var input = new MemoryStream();
            using var writer = new BinaryWriter(input);
            writer.Write(encryptedContent);
            writer.Flush();
            input.Seek(0, SeekOrigin.Begin);

            // Act
            var encryptor = new FileEncryptor();
            using var output = new MemoryStream();
            await encryptor.DecryptAsync(input, output, "password");

            var content = Encoding.UTF8.GetString(output.ToArray());

            // Assert
            Assert.NotEmpty(content);
            Assert.Equal("testing\r\n", content);
        }
    }
}
