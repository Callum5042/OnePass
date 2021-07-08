using OnePass.Handlers;
using OnePass.WPF.Models;
using OnePass.Services;
using OnePass.WPF.Tests;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
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

            // Act
            var model = new Product()
            {
                Id = 1,
                Name = "UpdatedName",
                Login = "UpdatedLogin",
                Password = "UpdatedPassword"
            };

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Filename = filename, MasterPassword = password };
            var handler = new UpdateProductHandler(fileSystem, encryptor, onePassRepository);
            await handler.UpdateAsync(model);

            // Assert
            var outputJson = fileSystem.File.ReadAllText(filename);
            var output = JsonSerializer.Deserialize<ProductRoot>(outputJson);

            var product = output.Products.First();
            Assert.NotNull(product);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
