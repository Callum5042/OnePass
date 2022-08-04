using OnePass.WPF.Models;
using Xunit;

namespace OnePass.WPF.Tests.Tests
{
    public class LoginValidationModelTests
    {
        [Fact]
        public void IsValid_UsernameMissing_UsernameIsRequiredAndReturnsFalse()
        {
            // Act
            var model = new LoginValidationModel()
            {
                Password = "Password123456789"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Equal("The Username field is required.", model.UsernameValidation);
            Assert.Null(model.PasswordValidation);
        }

        [Fact]
        public void IsValid_PasswordMissing_PasswordIsRequiredAndReturnsFalse()
        {
            // Act
            var model = new LoginValidationModel()
            {
                Username = "Username"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Equal("The Password field is required.", model.PasswordValidation);
        }

        [Fact]
        public void IsValid_PasswordTooShort_PasswordIsTooShortAndReturnsFalse()
        {
            // Act
            var model = new LoginValidationModel()
            {
                Username = "Username",
                Password = "password"
            };

            var result = model.IsValid();

            // Assert
            Assert.False(result);
            Assert.Equal("Password must be at least 10 characters.", model.PasswordValidation);
        }
    }
}
