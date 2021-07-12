using OnePass.Handlers;
using OnePass.WPF.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using OnePass.WPF.Tests;

namespace OnePass.Tests.Handlers
{
    public class LoginHandlerTests
    {
        [Fact]
        public async Task LoginAsync_ValidUsernameAndPassword_ReturnsSuccess()
        {
            // Arrange
            var username = "user";
            var filename = $"{username}.bin";

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(string.Empty) }
            });

            var onePassRepository = new OnePassRepository(); 
            var encryptor = new MockEncryptor();
            var handler = new LoginHandler(fileSystem, onePassRepository, encryptor);
            var result = await handler.LoginAsync(username, "password");

            // Assert
            Assert.Equal(LoginResult.Success, result);
            Assert.Equal("password", onePassRepository.MasterPassword);
        }

        [Fact]
        public async Task LoginAsync_UserMappingJsonFileDoesntExist_ReturnsInvalidUsername()
        {
            // Act
            var fileSystem = new MockFileSystem();
            var encryptor = new MockEncryptor();
            var handler = new LoginHandler(fileSystem, new OnePassRepository(), encryptor);
            var result = await handler.LoginAsync("username", "password");

            // Assert
            Assert.Equal(LoginResult.InvalidUsername, result);
        }

        [Fact]
        public async Task LoginAsync_UsernameDoesntExist_ReturnsInvalidUsername()
        {
            // Act
            var fileSystem = new MockFileSystem();

            var encryptor = new MockEncryptor();
            var handler = new LoginHandler(fileSystem, new OnePassRepository(), encryptor);
            var result = await handler.LoginAsync("username123", "password");

            // Assert
            Assert.Equal(LoginResult.InvalidUsername, result);
        }
    }
}
