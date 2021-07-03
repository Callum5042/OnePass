using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
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

            var filename = $"{nameof(RegisterAccountAsync_UsernameDoesNotExist_ReturnSuccess)}.json";
            using var fileCleanupFactory = new FileCleanupFactory(filename);

            var json = JsonSerializer.Serialize(accountRoot);
            await fileCleanupFactory.WriteAsync(json);

            // Act
            var hasher = new MockHasher();
            var encryptor = new Encryptor();
            var handler = new RegisterAccountHandler(hasher, encryptor) { Filename = filename };
            var result = await handler.RegisterAccountAsync("username123", "password");

            // Assert
            Assert.Equal(RegisterAccountResult.Success, result);
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

            var filename = $"{nameof(RegisterAccountAsync_UsernamealreadyExists_ReturnUsernameAlreadyExists)}.json";
            using var fileCleanupFactory = new FileCleanupFactory(filename);

            var json = JsonSerializer.Serialize(accountRoot);
            await fileCleanupFactory.WriteAsync(json);

            // Act
            var hasher = new MockHasher();
            var encryptor = new Encryptor();
            var handler = new RegisterAccountHandler(hasher, encryptor) { Filename = filename };
            var result = await handler.RegisterAccountAsync("username", "password");

            // Assert
            Assert.Equal(RegisterAccountResult.UsernameAlreadyExists, result);
        }
    }
}
