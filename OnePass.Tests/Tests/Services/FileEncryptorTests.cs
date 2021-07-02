using OnePass.Services;
using System.IO;
using System.Text;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public class FileEncryptorTests
    {
        [Fact]
        public void Encrypt()
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
            encryptor.Encrypt(input, output, "password");

            var content = output.ToArray();

            // Assert
            var encryptedContent = new byte[]
            {
                122, 173, 196, 6, 24, 91, 77, 249, 118, 155, 203, 50, 131, 232, 0, 21
            };

            Assert.NotEmpty(content);
            Assert.Equal(encryptedContent, content);
        }

        [Fact]
        public void Decrypt()
        {
            // Arrange
            var encryptedContent = new byte[]
            {
                122, 173, 196, 6, 24, 91, 77, 249, 118, 155, 203, 50, 131, 232, 0, 21
            };

            using var input = new MemoryStream();
            using var writer = new BinaryWriter(input);
            writer.Write(encryptedContent);
            writer.Flush();
            input.Seek(0, SeekOrigin.Begin);

            // Act
            var encryptor = new FileEncryptor();
            using var output = new MemoryStream();
            encryptor.Decrypt(input, output, "password");

            var content = Encoding.UTF8.GetString(output.ToArray());

            // Assert
            Assert.NotEmpty(content);
            Assert.Equal("testing\r\n", content);
        }
    }
}
