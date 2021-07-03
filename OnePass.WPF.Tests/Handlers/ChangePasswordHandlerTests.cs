using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Tests;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
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

            // Setup usermapping
            var usermapping = "usermapping.json";

            var accountRoot = new AccountRoot();
            accountRoot.Accounts.Add(new Account()
            {
                Username = "username",
                Password = "password"
            });

            var usermappingJson = JsonSerializer.Serialize(accountRoot);

            // Add data.bin
            var root = new ProductRoot() { Products = new List<Product>() };
            var json = JsonSerializer.Serialize(root);

            // Act
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { usermapping, new MockFileData(usermappingJson) },
                { filename, new MockFileData(json) },
            });

            var encryptor = new MockEncryptor();
            var onePassRepository = new OnePassRepository() { Username = "username", Filename = filename, MasterPassword = password };
            var hasher = new MockHasher();
            var handler = new ChangePasswordHandler(fileSystem, encryptor, onePassRepository, hasher) { Filename = usermapping };
            var result = await handler.ChangePassword(password, newPassword);

            // Assert
            Assert.True(result);

            var outputUsermappingJson = fileSystem.File.ReadAllText(usermapping);
            var outputUsermapping = JsonSerializer.Deserialize<AccountRoot>(outputUsermappingJson);

            Assert.NotNull(outputUsermapping);
            var user = outputUsermapping.Accounts.First();
            Assert.Equal(newPassword, user.Password);

            var file = fileSystem.File.ReadAllText(filename);
            var products = JsonSerializer.Deserialize<ProductRoot>(file);
            Assert.NotNull(products);
        }

        [Fact]
        public async Task ChangePassword_MasterDoNotPasswordMatches_ReturnFailedString()
        {
            var filename = "changedata2.bin";
            var password = "TestPassword";
            var newPassword = "NewTestPassword";

            // Arrange
            var root = new ProductRoot()
            {
                Products = new List<Product>()
            };

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
