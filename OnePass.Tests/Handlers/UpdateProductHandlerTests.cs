using OnePass.Handlers;
using OnePass.Services;
using System.Collections.Generic;
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
            var model = new Product()
            {
                Id = 1,
                Name = "UpdatedName",
                Login = "UpdatedLogin",
                Password = "UpdatedPassword"
            };

            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = "TestPassword" });
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
