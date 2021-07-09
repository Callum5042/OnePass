using OnePass.Handlers;
using OnePass.WPF.Models;
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
            // Act
            var fileSystem = new MockFileSystem();

            var encryptor = new MockEncryptor();
            var handler = new RegisterAccountHandler(fileSystem, encryptor);

            var username = "username123";
            var result = await handler.RegisterAccountAsync(username, "password");

            // Assert
            Assert.Equal(RegisterAccountResult.Success, result);

            var outputJson = fileSystem.File.ReadAllText($"{username}.bin");
            var output = JsonSerializer.Deserialize<List<Account>>(outputJson);

            Assert.Empty(output);
        }

        [Fact]
        public async Task RegisterAccountAsync_UsernamealreadyExists_ReturnUsernameAlreadyExists()
        {
            // Arrange
            var username = "username";
            var filename = $"{username}.bin";

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData("") }
            });

            var encryptor = new MockEncryptor();
            var handler = new RegisterAccountHandler(fileSystem, encryptor);
            var result = await handler.RegisterAccountAsync(username, "password");

            // Assert
            Assert.Equal(RegisterAccountResult.UsernameAlreadyExists, result);
        }
    }
}
