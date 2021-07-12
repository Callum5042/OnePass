using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Models;
using OnePass.WPF.Tests;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class ChangePasswordHandlerTests
    {
        [Fact]
        public async Task ChangePassword_MasterPasswordMatches_UpdatesAndEncryptFilesWithMasterPassword()
        {
            var filename = "data.bin";
            var password = "TestPassword";
            var newPassword = "NewTestPassword";

            // Arrange
            var root = new List<Account>() { new Account() };
            var json = JsonSerializer.Serialize(root);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) },
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Username = "username", Filename = filename, MasterPassword = password };
            var hasher = new MockHasher();
            var handler = new ChangePasswordHandler(fileSystem, encryptor, onePassRepository, hasher);
            var result = await handler.ChangePassword(password, newPassword);

            // Assert
            Assert.True(result);

            var file = fileSystem.File.ReadAllText(filename);
            var products = JsonSerializer.Deserialize<IList<Account>>(file);
            Assert.NotNull(products);
        }

        [Fact]
        public async Task ChangePassword_MasterDoNotPasswordMatches_ReturnFailedString()
        {
            var filename = "changedata2.bin";
            var password = "TestPassword";
            var newPassword = "NewTestPassword";

            // Arrange
            var root = new List<Account>() { new Account() };
            var json = JsonSerializer.Serialize(root);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filename, new MockFileData(json) }
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Username = "username", Filename = filename, MasterPassword = password };
            var hasher = new MockHasher();
            var handler = new ChangePasswordHandler(fileSystem, encryptor, onePassRepository, hasher);
            var result = await handler.ChangePassword(newPassword, newPassword);

            // Assert
            Assert.False(result);
        }
    }
}
