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
    public class AddProductHandlerTests
    {
        [Fact]
        public async Task AddProductAsync()
        {
            var filename = "data.bin";
            var password = "TestPassword";

            // Arrange
            var root = new ProductRoot()
            {
                Products = new List<Product>()
            };

            var json = JsonSerializer.Serialize(root);

            // Act
            var model = new Product()
            {
                Name = "Callum",
                Login = "Login",
                Password = "password"
            };

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Filename = filename, MasterPassword = password };
            var handler = new AddProductHandler(fileSystem, encryptor, onePassRepository);
            var result = await handler.AddProductAsync(model);

            // Assert
            var outputJson = fileSystem.File.ReadAllText(filename);
            var output = JsonSerializer.Deserialize<ProductRoot>(outputJson);

            var product = output.Products.First();
            Assert.NotNull(product);
            Assert.Equal(1, product.Id);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
