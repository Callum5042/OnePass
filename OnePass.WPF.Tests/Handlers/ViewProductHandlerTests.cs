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
    public class ViewProductHandlerTests
    {
        [Fact]
        public async Task GetAllProductsAsync()
        {
            var filename = "data.bin";
            var password = "TestPassword";

            // Arrange
            var account = new Account()
            {
                Id = 1,
                Name = "Callum",
                Login = "Login",
                Password = "password"
            };

            var accounts = new List<Account>() { account };
            var data = JsonSerializer.Serialize(accounts);

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
            Assert.Equal(account.Id, product.Id);
            Assert.Equal(account.Name, product.Name);
            Assert.Equal(account.Login, product.Login);
            Assert.Equal(account.Password, product.Password);
        }
    }
}
