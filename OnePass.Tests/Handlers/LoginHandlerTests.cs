using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services.Interfaces;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class LoginHandlerTests
    {
        private class TestHasher : IHasher
        {
            public string ComputeHashToString(string value) => value;
        }

        private static async Task CreateUserMapping(AccountRoot accountRoot)
        {
            using var file = File.OpenWrite(@"usermapping.json");
            using var writer = new StreamWriter(file);

            var json = JsonSerializer.Serialize(accountRoot);
            await writer.WriteAsync(json);
        }

        [Fact]
        public async Task Login_ValidUsernameAndPassword_ReturnsSuccess()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            await CreateUserMapping(accountRoot);

            // Act
            var hasher = new TestHasher();
            var handler = new LoginHandler(hasher);
            var result = await handler.LoginAsync("username", "password");

            // Assert
            Assert.Equal(LoginResult.Success, result);
        }

        [Fact]
        public async Task Login_UserMappingJsonFileDoesntExist_ReturnsInvalidUsername()
        {
            // Arrange
            if (File.Exists(@"usermapping.json"))
            {
                File.Delete(@"usermapping.json");
            }

            // Act
            var hasher = new TestHasher();
            var handler = new LoginHandler(hasher);
            var result = await handler.LoginAsync("username", "password");

            // Assert
            Assert.Equal(LoginResult.InvalidUsername, result);
        }

        [Fact]
        public async Task Login_UsernameDoesntExist_ReturnsInvalidUsername()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            await CreateUserMapping(accountRoot);

            // Act
            var hasher = new TestHasher();
            var handler = new LoginHandler(hasher);
            var result = await handler.LoginAsync("username123", "password");

            // Assert
            Assert.Equal(LoginResult.InvalidUsername, result);
        }

        [Fact]
        public async Task Login_PasswordDoesNotMatch_ReturnsInvalidPassword()
        {
            // Arrange
            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            await CreateUserMapping(accountRoot);

            // Act
            var hasher = new TestHasher();
            var handler = new LoginHandler(hasher);
            var result = await handler.LoginAsync("username", "password123");

            // Assert
            Assert.Equal(LoginResult.InvalidPassword, result);
        }
    }
}
