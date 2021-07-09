using OnePass.Handlers;
using OnePass.Services;
using OnePass.WPF.Models;
using OnePass.WPF.Tests;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Account = OnePass.Models.Account;

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
            var accounts = new List<Account>()
            {
                new Account()
                {
                    Id = 1,
                    Name = "Callum",
                    Login = "Login",
                    Password = "password"
                },
                new Account()
                {
                    Id = 2,
                    Name = "Callum",
                    Login = "Login",
                    Password = "password"
                }
            };

            var json = JsonSerializer.Serialize(accounts);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Filename = filename, MasterPassword = password };
            var handler = new DeleteProductHandler(fileSystem, encryptor, onePassRepository);
            await handler.DeleteProductAsync(new Product()
            {
                Id = 1,
                Name = "Callum"
            });

            // Assert
            var outputJson = fileSystem.File.ReadAllText(filename);
            var output = JsonSerializer.Deserialize<IList<Account>>(outputJson);

            Assert.Single(output);
        }
    }
}
