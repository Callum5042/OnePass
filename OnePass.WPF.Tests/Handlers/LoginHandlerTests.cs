using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class LoginHandlerTests
    {
        [Fact]
        public async Task LoginAsync_ValidUsernameAndPassword_ReturnsSuccess()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            var filename = $"{nameof(LoginAsync_ValidUsernameAndPassword_ReturnsSuccess)}.json";
            using var fileCleanupFactory = new FileCleanupFactory(filename);

            var json = JsonSerializer.Serialize(accountRoot);
            await fileCleanupFactory.WriteAsync(json);

            // Act
            var hasher = new MockHasher();
            var onePassRepository = new OnePassRepository();
            var handler = new LoginHandler(hasher, onePassRepository) { Filename = filename };
            var result = await handler.LoginAsync("username", "password");

            // Assert
            Assert.Equal(LoginResult.Success, result);
            Assert.Equal("password", onePassRepository.MasterPassword);
        }

        [Fact]
        public async Task LoginAsync_UserMappingJsonFileDoesntExist_ReturnsInvalidUsername()
        {
            // Arrange
            if (File.Exists(@"usermapping.json"))
            {
                File.Delete(@"usermapping.json");
            }

            // Act
            var hasher = new MockHasher();
            var handler = new LoginHandler(hasher, new OnePassRepository());
            var result = await handler.LoginAsync("username", "password");

            // Assert
            Assert.Equal(LoginResult.InvalidUsername, result);
        }

        [Fact]
        public async Task LoginAsync_UsernameDoesntExist_ReturnsInvalidUsername()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            var filename = $"{nameof(LoginAsync_UsernameDoesntExist_ReturnsInvalidUsername)}.json";
            using var fileCleanupFactory = new FileCleanupFactory(filename);

            var json = JsonSerializer.Serialize(accountRoot);
            await fileCleanupFactory.WriteAsync(json);

            // Act
            var hasher = new MockHasher();
            var handler = new LoginHandler(hasher, new OnePassRepository()) { Filename = filename };
            var result = await handler.LoginAsync("username123", "password");

            // Assert
            Assert.Equal(LoginResult.InvalidUsername, result);
        }

        [Fact]
        public async Task LoginAsync_PasswordDoesNotMatch_ReturnsInvalidPassword()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            var filename = $"{nameof(LoginAsync_PasswordDoesNotMatch_ReturnsInvalidPassword)}.json";
            using var fileCleanupFactory = new FileCleanupFactory(filename);

            var json = JsonSerializer.Serialize(accountRoot);
            await fileCleanupFactory.WriteAsync(json);

            // Act
            var hasher = new MockHasher();
            var handler = new LoginHandler(hasher, new OnePassRepository()) { Filename = filename };
            var result = await handler.LoginAsync("username", "password123");

            // Assert
            Assert.Equal(LoginResult.InvalidPassword, result);
        }
    }
}
