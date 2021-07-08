using OnePass.Handlers;
using OnePass.WPF.Models;
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
    public class DeleteProductHandlerTests
    {
        [Fact]
        public async Task DeleteProductAsync()
        {
            var filename = "data.bin";
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

            // Act
            var product = root.Products.First();

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Filename = filename, MasterPassword = password };
            var handler = new DeleteProductHandler(fileSystem, encryptor, onePassRepository);
            await handler.DeleteProductAsync(product);

            // Assert
            var outputJson = fileSystem.File.ReadAllText(filename);
            var output = JsonSerializer.Deserialize<ProductRoot>(outputJson);

            Assert.Single(output.Products);
        }
    }
}
