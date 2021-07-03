using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Tests;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
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
            var filename = "data.bin";
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

            var data = JsonSerializer.Serialize(root);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(data) }
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Filename = filename, MasterPassword = password };
            var handler = new ViewProductHandler(fileSystem, encryptor, onePassRepository);
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
