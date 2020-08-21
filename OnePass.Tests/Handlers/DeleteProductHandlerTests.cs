using OnePass.Handlers;
using OnePass.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class DeleteProductHandlerTests
    {
        [Fact]
        public async Task DeleteProductAsync()
        {
            var filename = "deletedata.bin";

            // Arrange
            var root = new ProductRoot()
            {
                Products = new List<Product>()
                {
                    new Product()
                    {
                        Id = 1,
                        Name = "Callum",
                        Login = "Login",
                        Password = "password"
                    }
                }
            };

            var json = JsonSerializer.Serialize(root);

            var encryptor = new Encryptor();
            await encryptor.EncryptAsync(filename, "TestPassword", json);

            // Act
            var product = root.Products.First();

            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = "TestPassword" });
            var handler = new DeleteProductHandler(encryptor, settings);
            var result = await handler.DeleteProductAsync(product);

            // Assert
            Assert.Empty(result);
        }
    }
}
