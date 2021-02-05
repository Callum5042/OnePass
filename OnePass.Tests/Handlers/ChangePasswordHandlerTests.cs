using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
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
            var filename = "changedata.bin";
            var password = "TestPassword";
            var newPassword = "NewTestPassword";

            // Arrange
            var root = new ProductRoot()
            {
                Products = new List<Product>()
            };

            var json = JsonSerializer.Serialize(root);

            using var encryptCleanupFactory = new EncryptorCleanupFactory(filename);
            await encryptCleanupFactory.Encrypt(password, json);

            // Act
            var encryptor = new Encryptor();
            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = password });
            var handler = new ChangePasswordHandler(encryptor, settings);
            var result = await handler.ChangePassword(password, newPassword);

            // Assert
            Assert.Equal("Password has been changed", result);
            Assert.Equal(settings.Current.MasterPassword, newPassword);
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

            using var encryptCleanupFactory = new EncryptorCleanupFactory(filename);
            await encryptCleanupFactory.Encrypt(password, json);

            // Act
            var encryptor = new Encryptor();
            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = password });
            var handler = new ChangePasswordHandler(encryptor, settings);
            var result = await handler.ChangePassword(newPassword, newPassword);

            // Assert
            Assert.Equal("Current password is invalid", result);
            Assert.Equal(settings.Current.MasterPassword, password);
        }
    }
}
