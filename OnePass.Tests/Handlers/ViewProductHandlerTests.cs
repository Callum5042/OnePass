using OnePass.Handlers;
using OnePass.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public partial class ViewProductHandlerTests
    {
        [Fact]
        public async Task GetAllProductsAsync()
        {
            // Act
            var encryptor = new TestEncryptor();
            var settings = new TestSettingsMonitor(new OnePassSettings());
            var handler = new ViewProductHandler(encryptor, settings);
            var result = await handler.GetAllProductsAsync();

            // Assert
            Assert.Single(result);
        }

        public class TestEncryptor : IEncryptor
        {
            public Task<string> DecryptAsync(string file, string password)
            {
                var json = "{\"Products\":[{\"Id\": 1,\"Name\": \"TestName\",\"Login\": \"TestLogin\",\"Password\": \"password123\"}]}";
                return Task.FromResult(json);
            }

            public Task EncryptAsync(string file, string password, string data)
            {
                throw new NotImplementedException();
            }
        }
    }
}
