using OnePass.Services;
using System;
using Xunit;

namespace OnePass.Tests.Services
{
    public class PasswordGeneratorTests
    {
        [Fact]
        public void Generate_OptionsIsNull_ThrowsArgumentNullException()
        {
            // Act
            var generator = new PasswordGenerator();

            // Assert
            Assert.Throws<ArgumentNullException>(() => generator.Generate(null));
        }

        [Fact]
        public void Generate_LowercaseIsTrue_ReturnsLowercasePassword()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = false,
                Lowercase = true,
                Uppercase = false,
                Symbols = false,
                MinLength = 8,
                MaxLength = 8
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(8, result.Length);
            Assert.All(result, x => char.IsLower(x));
        }

        [Fact]
        public void Generate_UppercaseIsTrue_ReturnsUppercasePassword()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = false,
                Lowercase = false,
                Uppercase = true,
                Symbols = false,
                MinLength = 8,
                MaxLength = 8
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(8, result.Length);
            Assert.All(result, x => char.IsUpper(x));
        }

        [Fact]
        public void Generate_NumbersIsTrue_ReturnsNumberPassword()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = true,
                Lowercase = false,
                Uppercase = false,
                Symbols = false,
                MinLength = 8,
                MaxLength = 8
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(8, result.Length);
            Assert.All(result, x => char.IsDigit(x));
        }

        [Fact]
        public void Generate_UppercaseIsTrueAndLowercaseIsTrue_ReturnsMixCasePassword()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = false,
                Lowercase = true,
                Uppercase = true,
                Symbols = false,
                MinLength = 16,
                MaxLength = 16
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(16, result.Length);
            Assert.All(result, x => char.IsLetter(x));
            Assert.Contains(result, x => char.IsUpper(x));
            Assert.Contains(result, x => char.IsLower(x));
        }

        // Test is a bit dodgey and randomly fails - may have to be removed or further investigated
        [Fact]
        public void Generate_UppercaseIsTrueAndLowercaseIsTrueAndNumbersIsTrue_ReturnsAlphanumericMixCasePassword()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = true,
                Lowercase = true,
                Uppercase = true,
                Symbols = false,
                MinLength = 16,
                MaxLength = 16
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(16, result.Length);
            Assert.All(result, x => char.IsLetterOrDigit(x));
            Assert.Contains(result, x => char.IsUpper(x));
            Assert.Contains(result, x => char.IsLower(x));
            Assert.Contains(result, x => char.IsDigit(x));
        }

        [Fact]
        public void Generate_MinAndMaxLengthDifferentSizes_ReturnsPasswordWithinRange()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = false,
                Lowercase = true,
                Uppercase = false,
                Symbols = false,
                MinLength = 8,
                MaxLength = 16
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.InRange(result.Length, 8, 16);
        }

        [Fact]
        public void Generate_Symbols_ReturnsPasswordWithSymbols()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = false,
                Lowercase = false,
                Uppercase = false,
                Symbols = true,
                MinLength = 16,
                MaxLength = 16
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, x => char.IsSymbol(x));
        }

        [Fact]
        public void Generate_SymbolsExludeCertainSymbol_ReturnsPasswordWithSymbolsExcludingExcluded()
        {
            // Act
            var generator = new PasswordGenerator();
            var result = generator.Generate(new PasswordGeneratorOptions()
            {
                Numbers = false,
                Lowercase = false,
                Uppercase = false,
                Symbols = true,
                MinLength = 16,
                MaxLength = 16,
                ExcludeSymbolList = "{"
            });

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, x => char.IsSymbol(x));
            Assert.DoesNotContain(result, x => x == '{');
        }
    }
}
