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
using Account = OnePass.Models.Account;

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
            var accounts = new List<Account>();
            var json = JsonSerializer.Serialize(accounts);

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
            await handler.AddProductAsync(model);

            // Assert
            var outputJson = fileSystem.File.ReadAllText(filename);
            var output = JsonSerializer.Deserialize<IList<Account>>(outputJson);

            var product = output.First();
            Assert.NotNull(product);
            Assert.Equal(1, product.Id);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
