using OnePass.Services;
using System;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public class PasswordGeneratorTests
    {
        [Fact]
        public void Generate_IsCasesensitiveWithNumbersAndSymbols_ContainsAllRequirements()
        {
            var symbols = "[]{};:@#~<>/!?";

            // Act
            var generator = new PasswordGenerator()
            {
                HasLowercase = true,
                HasUppercase = true,
                HasNumbers = true,
                HasSymbols = true,
                MinLength = 10,
                MaxLength = 10
            };

            var password = generator.Generate();

            // Assert
            Assert.NotEmpty(password);
            Assert.Equal(generator.MinLength, password.Length);
            Assert.Equal(generator.MaxLength, password.Length);
            Assert.Contains(password, x => char.IsUpper(x));
            Assert.Contains(password, x => char.IsLower(x));
            Assert.Contains(password, x => char.IsDigit(x));
            Assert.Contains(password, x => symbols.Contains(x));
        }

        [Fact]
        public void Generate_MaxLengthGreaterThan100_ThrowsArgumentException()
        {
            // Act & Assert
            var generator = new PasswordGenerator()
            {
                HasLowercase = true,
                HasUppercase = true,
                HasNumbers = true,
                HasSymbols = true,
                MaxLength = 200
            };

            Assert.Throws<ArgumentException>(() => generator.Generate());
        }

        [Fact]
        public void Generate_MinLengthGreaterThanMaxLength_ThrowsArgumentException()
        {
            // Act & Assert
            var generator = new PasswordGenerator()
            {
                HasLowercase = true,
                HasUppercase = true,
                HasNumbers = true,
                HasSymbols = true,
                MinLength = 30,
                MaxLength = 10,
            };

            Assert.Throws<ArgumentException>(() => generator.Generate());
        }

        [Fact]
        public void Generate_AllCasesAreFalse_ThrowsArgumentException()
        {
            // Act & Assert
            var generator = new PasswordGenerator()
            {
                HasLowercase = false,
                HasUppercase = false,
                HasNumbers = false,
                HasSymbols = false,
                MinLength = 5,
                MaxLength = 10,
            };

            Assert.Throws<ArgumentException>(() => generator.Generate());
        }
    }
}
