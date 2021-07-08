using OnePass.Handlers;
using OnePass.WPF.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
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

            var filename = "usermapping.json";
            var json = JsonSerializer.Serialize(accountRoot);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var hasher = new MockHasher();
            var onePassRepository = new OnePassRepository();
            var handler = new LoginHandler(fileSystem, hasher, onePassRepository) { Filename = filename };
            var result = await handler.LoginAsync("username", "password");

            // Assert
            Assert.Equal(LoginResult.Success, result);
            Assert.Equal("password", onePassRepository.MasterPassword);
        }

        [Fact]
        public async Task LoginAsync_UserMappingJsonFileDoesntExist_ReturnsInvalidUsername()
        {
            // Act
            var fileSystem = new MockFileSystem();
            var hasher = new MockHasher();
            var handler = new LoginHandler(fileSystem, hasher, new OnePassRepository());
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

            var filename = "usermapping.json";
            var json = JsonSerializer.Serialize(accountRoot);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var hasher = new MockHasher();
            var handler = new LoginHandler(fileSystem, hasher, new OnePassRepository()) { Filename = filename };
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

            var filename = "usermapping.json";
            var json = JsonSerializer.Serialize(accountRoot);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var hasher = new MockHasher();
            var handler = new LoginHandler(fileSystem, hasher, new OnePassRepository()) { Filename = filename };
            var result = await handler.LoginAsync("username", "password123");

            // Assert
            Assert.Equal(LoginResult.InvalidPassword, result);
        }
    }
}
