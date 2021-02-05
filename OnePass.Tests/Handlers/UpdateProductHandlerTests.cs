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
    public class UpdateProductHandlerTests
    {
        [Fact]
        public async Task GetAllProductsAsync()
        {
            var filename = "updatedata.bin";
            var password = "TestPassword";

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

            using var encryptCleanupFactory = new EncryptorCleanupFactory(filename);
            await encryptCleanupFactory.Encrypt(password, json);

            // Act
            var model = new Product()
            {
                Id = 1,
                Name = "UpdatedName",
                Login = "UpdatedLogin",
                Password = "UpdatedPassword"
            };

            var encryptor = new Encryptor();
            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = password });
            var handler = new UpdateProductHandler(encryptor, settings);
            var result = await handler.UpdateAsync(model);

            // Assert
            Assert.Single(result);

            var product = result.First(x => x.Id == model.Id);
            Assert.NotNull(product);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
