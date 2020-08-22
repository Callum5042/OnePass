﻿using OnePass.Handlers;
using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Handlers
{
    public class AddProductHandlerTests
    {
        [Fact]
        public async Task GetAllProductsAsync()
        {
            var filename = "adddata.bin";

            // Arrange
            var root = new ProductRoot()
            {
                Products = new List<Product>()
            };

            var json = JsonSerializer.Serialize(root);

            var encryptor = new Encryptor();
            await encryptor.EncryptAsync(filename, "TestPassword", json);

            // Act
            var model = new Product()
            {
                Name = "Callum",
                Login = "Login",
                Password = "password"
            };

            var settings = new TestSettingsMonitor(new OnePassSettings() { FileName = filename, MasterPassword = "TestPassword" });
            var handler = new AddProductHandler(encryptor, settings);
            var result = await handler.AddProduct(model);

            // Assert
            Assert.Single(result);

            var product = result.First();
            Assert.Equal(1, product.Id);
            Assert.Equal(model.Name, product.Name);
            Assert.Equal(model.Login, product.Login);
            Assert.Equal(model.Password, product.Password);
        }
    }
}
