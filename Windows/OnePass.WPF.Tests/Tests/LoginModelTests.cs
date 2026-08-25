using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.WPF.Tests.Tests
{
    public class LoginModelTests
    {
        private const string Password = "Password123456789";

        [Fact]
        public async Task TryDecryptAsync_ValidVaultOutsideWorkingDirectory_ReturnsVault()
        {
            var directory = CreateTempDirectory();
            var filePath = Path.Combine(directory, "selected-vault.bin");
            var encoder = new FileEncoder();
            var vault = new OnePassData();
            vault.Accounts.Add(new Account { Name = "Example" });

            try
            {
                await encoder.SaveAsync("ignored", Password, vault, filePath);
                var model = CreateModel(encoder, filePath, Password);

                var result = await model.TryDecryptAsync();

                Assert.NotNull(result);
                Assert.Single(result.Accounts);
                Assert.Null(model.Login.FilePathValidation);
                Assert.Null(model.Login.PasswordValidation);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public async Task TryDecryptAsync_IncorrectPassword_SetsPasswordValidation()
        {
            var directory = CreateTempDirectory();
            var filePath = Path.Combine(directory, "selected-vault.bin");
            var encoder = new FileEncoder();

            try
            {
                await encoder.SaveAsync("ignored", Password, new OnePassData(), filePath);
                var model = CreateModel(encoder, filePath, "IncorrectPassword123");

                var result = await model.TryDecryptAsync();

                Assert.Null(result);
                Assert.Equal("Password is incorrect.", model.Login.PasswordValidation);
                Assert.Null(model.Login.FilePathValidation);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public async Task TryDecryptAsync_InvalidVault_SetsFileValidation()
        {
            var directory = CreateTempDirectory();
            var filePath = Path.Combine(directory, "invalid.bin");
            File.WriteAllText(filePath, "not a OnePass vault");

            try
            {
                var model = CreateModel(new FileEncoder(), filePath, Password);

                var result = await model.TryDecryptAsync();

                Assert.Null(result);
                Assert.Equal("Not a valid OnePass file.", model.Login.FilePathValidation);
                Assert.Null(model.Login.PasswordValidation);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public void ApplyOptions_RememberedFileExists_SelectsFile()
        {
            var directory = CreateTempDirectory();
            var filePath = Path.Combine(directory, "remembered.bin");
            File.WriteAllBytes(filePath, Array.Empty<byte>());

            try
            {
                var options = new AppOptions { RememberFilePath = filePath };
                var model = new LoginModel(new FileEncoder());

                model.ApplyOptions(options);

                Assert.Equal(Path.GetFullPath(filePath), model.Login.FilePath);
                Assert.Equal("remembered.bin", model.Login.FileName);
                Assert.True(model.Login.RememberMe);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public void ApplyOptions_RememberedFileMissing_DoesNotSelectFile()
        {
            var options = new AppOptions
            {
                RememberFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bin")
            };
            var model = new LoginModel(new FileEncoder());

            model.ApplyOptions(options);

            Assert.Null(model.Login.FilePath);
            Assert.False(model.Login.RememberMe);
        }

        [Fact]
        public void ApplyOptions_LegacyUsername_MigratesToFilePath()
        {
            var directory = CreateTempDirectory();
            var filePath = Path.Combine(directory, "legacy.bin");
            File.WriteAllBytes(filePath, Array.Empty<byte>());

            try
            {
                var options = new AppOptions { RememberUsername = "legacy" };
                var model = new LoginModel(new FileEncoder());

                model.ApplyOptions(options, directory);

                Assert.Equal(Path.GetFullPath(filePath), options.RememberFilePath);
                Assert.Equal(string.Empty, options.RememberUsername);
                Assert.Equal(Path.GetFullPath(filePath), model.Login.FilePath);
                Assert.True(model.Login.RememberMe);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public void UpdateRememberedFileOption_Checked_StoresOnlyFullFilePath()
        {
            var filePath = Path.Combine(Path.GetTempPath(), "vault.bin");
            var options = new AppOptions { RememberUsername = "legacy" };
            var model = CreateModel(new FileEncoder(), filePath, Password);
            model.Login.RememberMe = true;

            model.UpdateRememberedFileOption(options);

            Assert.Equal(Path.GetFullPath(filePath), options.RememberFilePath);
            Assert.Equal(string.Empty, options.RememberUsername);
        }

        [Fact]
        public void UpdateRememberedFileOption_Unchecked_ClearsRememberedFile()
        {
            var options = new AppOptions
            {
                RememberFilePath = @"C:\vault.bin",
                RememberUsername = "legacy"
            };
            var model = new LoginModel(new FileEncoder());

            model.UpdateRememberedFileOption(options);

            Assert.Equal(string.Empty, options.RememberFilePath);
            Assert.Equal(string.Empty, options.RememberUsername);
        }

        private static LoginModel CreateModel(IFileEncoder encoder, string filePath, string password)
        {
            return new LoginModel(encoder)
            {
                Login = new LoginValidationModel
                {
                    FilePath = filePath,
                    Password = password
                }
            };
        }

        private static string CreateTempDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"OnePass-{Guid.NewGuid()}");
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
