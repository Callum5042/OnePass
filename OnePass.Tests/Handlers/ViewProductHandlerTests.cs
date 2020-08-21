﻿using OnePass.Handlers;
using OnePass.Services;
using System.Collections.Generic;
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

            // Arrange
            var root = new ProductRoot()
            {
                Products = new List<Product>()
                {
                    new Product()
                    {
                        Name = "Callum",
                        Login = "Login",
                        Password = "password"
                    }
                }
            };

            var json = JsonSerializer.Serialize(root);

            var encryptor = new Encryptor();
            await encryptor.EncryptAsync(filename, "TestPassword", json);

            // Act
            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = "TestPassword" });
            var handler = new ViewProductHandler(encryptor, settings);
            var result = await handler.GetAllProductsAsync();

            // Assert
            Assert.Single(result);
        }
    }
}
