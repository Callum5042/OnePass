using OnePass.WPF.Models;
using Xunit;

namespace OnePass.WPF.Tests.Tests
{
    public class RegisterValidationModelTests
    {
        [Fact]
        public void IsValid_NothingSet_FailsValidation()
        {
            // Act
            var model = new RegisterValidationModel();
            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Equal("The Filename field is required.", model.FilePathValidation);
            Assert.Equal("The Password field is required.", model.PasswordValidation);
            Assert.Equal("The Repeat Password field is required.", model.RepeatPasswordValidation);
        }

        [Fact]
        public void IsValid_UsernameIsSet_UsernameValidationFails()
        {
            // Act
            var model = new RegisterValidationModel()
            {
                FilePath = "Callum"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Null(model.FilePathValidation);
            Assert.Equal("The Password field is required.", model.PasswordValidation);
            Assert.Equal("The Repeat Password field is required.", model.RepeatPasswordValidation);
        }

        [Fact]
        public void IsValid_UsernameAndPasswordIsSet_RepeatPasswordValidationFails()
        {
            // Act
            var model = new RegisterValidationModel()
            {
                FilePath = "Callum",
                Password = "Password123456"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Null(model.FilePathValidation);
            Assert.Null(model.PasswordValidation);
            Assert.Equal("The Repeat Password field is required.", model.RepeatPasswordValidation);
        }

        [Fact]
        public void IsValid_PasswordDoNotMatch_RepeatPasswordValidationFails()
        {
            // Act
            var model = new RegisterValidationModel()
            {
                FilePath = "Callum",
                Password = "Password12345689",
                RepeatPassword = "PasswordPassword"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Null(model.FilePathValidation);
            Assert.Null(model.PasswordValidation);
            Assert.Equal("Passwords do not match.", model.RepeatPasswordValidation);
        }

        [Fact]
        public void IsValid_PasswordTooShort_PasswordValidationFails()
        {
            // Act
            var model = new RegisterValidationModel()
            {
                FilePath = "Callum",
                Password = "password",
                RepeatPassword = "password"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Null(model.FilePathValidation);
            Assert.Equal("Password must be at least 10 characters.", model.PasswordValidation);
            Assert.Null(model.RepeatPasswordValidation);
        }
    }
}
