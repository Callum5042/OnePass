using OnePass.WPF.Models;
using Xunit;
using System;
using System.IO;

namespace OnePass.WPF.Tests.Tests
{
    public class LoginValidationModelTests
    {
        [Fact]
        public void IsValid_FilenameMissing_FilenameIsRequiredAndReturnsFalse()
        {
            // Act
            var model = new LoginValidationModel()
            {
                Password = "Password123456789"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Equal("The Filename field is required.", model.FilePathValidation);
            Assert.Null(model.PasswordValidation);
        }

        [Fact]
        public void IsValid_FileDoesNotExist_FileValidationFails()
        {
            // Act
            var model = new LoginValidationModel()
            {
                FilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bin"),
                Password = "Password123456789"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.EndsWith("could not be found.", model.FilePathValidation);
            Assert.Null(model.PasswordValidation);
        }

        [Fact]
        public void IsValid_PasswordMissing_PasswordIsRequiredAndReturnsFalse()
        {
            var filePath = Path.GetTempFileName();

            try
            {
                // Act
                var model = new LoginValidationModel()
                {
                    FilePath = filePath
                };

                var result = model.IsValid();

                // Assert
                Assert.False(result);
                Assert.Null(model.FilePathValidation);
                Assert.Equal("The Password field is required.", model.PasswordValidation);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void IsValid_PasswordTooShort_PasswordIsTooShortAndReturnsFalse()
        {
            var filePath = Path.GetTempFileName();

            try
            {
                // Act
                var model = new LoginValidationModel()
                {
                    FilePath = filePath,
                    Password = "password"
                };

                var result = model.IsValid();

                // Assert
                Assert.False(result);
                Assert.Null(model.FilePathValidation);
                Assert.Equal("Password must be at least 10 characters.", model.PasswordValidation);
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
