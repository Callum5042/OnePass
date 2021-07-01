using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class ViewProductHandlerTests
    {
        [Fact]
        public async Task GetAllProductsAsync()
        {
            var filename = "viewdata.bin";
            var password = "TestPassword";

            // Arrange
            var model = new Product()
            {
                Id = 1,
                Name = "Callum",
                Login = "Login",
                Password = "password"
            };

            var root = new ProductRoot()
            {
                Products = new List<Product>() { model }
            };

            var json = JsonSerializer.Serialize(root);
            using var encryptCleanupFactory = new EncryptorCleanupFactory(filename);
            await encryptCleanupFactory.Encrypt(password, json);

            // Act
            var encryptor = new Encryptor();
            var onePassRepository = new OnePassRepository() { Filename = filename, MasterPassword = password };
            var handler = new ViewProductHandler(encryptor, onePassRepository);
            var result = await handler.GetAllProductsAsync();

            // Assert
            Assert.Single(result);

            var product = result.First();
            Assert.Equal(model.Id, product.Id);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
