using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.IO;
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
            var password = "TestPassword";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

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
                    },
                    new Product()
                    {
                        Id = 2,
                        Name = "Callum",
                        Login = "Login",
                        Password = "password"
                    }
                }
            };

            var json = JsonSerializer.Serialize(root);

            var encryptor = new Encryptor();
            await encryptor.EncryptAsync(filename, password, json);

            // Act
            var product = root.Products.First();

            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = password });
            var handler = new DeleteProductHandler(encryptor, settings);
            var result = await handler.DeleteProductAsync(product);

            // Assert
            Assert.Single(result);
        }
    }
}
