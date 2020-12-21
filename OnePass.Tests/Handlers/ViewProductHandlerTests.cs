using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class ViewProductHandlerTests
    {
        [Fact]
        public async Task GetAllProductsAsync()
        {
            var filename = "viewdata.bin";
            var password = "TestPassword";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            // Arrange
            var model = new Product()
            {
                Id = 1,
                Name = "Callum",
                Login = "Login",
                Password = "password"
            };

            var root = new ProductRoot()
            {
                Products = new List<Product>() { model }
            };

            var json = JsonSerializer.Serialize(root);

            var encryptor = new Encryptor();
            await encryptor.EncryptAsync(filename, password, json);

            // Act
            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = password });
            var handler = new ViewProductHandler(encryptor, settings);
            var result = await handler.GetAllProductsAsync();

            // Assert
            Assert.Single(result);

            var product = result.First();
            Assert.Equal(model.Id, product.Id);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
