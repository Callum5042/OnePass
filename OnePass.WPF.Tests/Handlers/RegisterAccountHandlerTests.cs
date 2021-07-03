using OnePass.Handlers;
using OnePass.Models;
using OnePass.WPF.Tests;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class RegisterAccountHandlerTests
    {
        [Fact]
        public async Task RegisterAccountAsync_UsernameDoesNotExist_ReturnSuccess()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            var filename = "usermapping.json";
            var json = JsonSerializer.Serialize(accountRoot);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var hasher = new MockHasher();
            var encryptor = new MockEncryptor();
            var handler = new RegisterAccountHandler(fileSystem, hasher, encryptor) { Filename = filename };

            var username = "username123";
            var result = await handler.RegisterAccountAsync(username, "password");

            // Assert
            Assert.Equal(RegisterAccountResult.Success, result);

            var outputJson = fileSystem.File.ReadAllText($"{username}.bin");
            var output = JsonSerializer.Deserialize<ProductRoot>(outputJson);

            Assert.NotNull(output);
            Assert.Empty(output.Products);
        }

        [Fact]
        public async Task RegisterAccountAsync_UsernamealreadyExists_ReturnUsernameAlreadyExists()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            var filename = "usermapping.json";
            var json = JsonSerializer.Serialize(accountRoot);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var hasher = new MockHasher();
            var encryptor = new MockEncryptor();
            var handler = new RegisterAccountHandler(fileSystem, hasher, encryptor) { Filename = filename };
            var result = await handler.RegisterAccountAsync("username", "password");

            // Assert
            Assert.Equal(RegisterAccountResult.UsernameAlreadyExists, result);
        }
    }
}
