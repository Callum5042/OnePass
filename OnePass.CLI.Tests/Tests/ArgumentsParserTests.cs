using System;
using Xunit;

namespace OnePass.CLI.Tests.Tests
{
    public class ArgumentsParserTests : TestSetup
    {
        [Fact]
        public void Parse_DecryptArguments_ReturnArguments()
        {
            // Arrange
            var argument = "-decrypt -password super -file testing.bin";
            var args = argument.Split(" ");

            // Act
            var parser = GetService<ArgumentsParser>();
            var result = parser.Parse(args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(CommandType.Decrypt, result.CommandType);
            Assert.Equal("super", result.Password);
            Assert.Equal("testing.bin", result.File);
        }

        [Fact]
        public void Parse_EncryptArguments_ReturnArguments()
        {
            // Arrange
            var argument = "-encrypt -file \"testing.bin\" -password \"super\"";
            var args = argument.Split(" ");

            // Act
            var parser = GetService<ArgumentsParser>();
            var result = parser.Parse(args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(CommandType.Encrypt, result.CommandType);
            Assert.Equal("super", result.Password);
            Assert.Equal("testing.bin", result.File);
        }

        [Fact]
        public void Parse_BadCommandType_ThrowsArgumentException()
        {
            // Arrange
            var argument = "-endw";
            var args = argument.Split(" ");

            // Act & Assert
            var parser = GetService<ArgumentsParser>();
            Assert.Throws<ArgumentException>(() => parser.Parse(args));
        }

        [Fact]
        public void Parse_MissingPassword_ThrowsArgumentException()
        {
            // Arrange
            var argument = "-decrypt -file testing.bin -password";
            var args = argument.Split(" ");

            // Act & Assert
            var parser = GetService<ArgumentsParser>();
            Assert.Throws<ArgumentException>(() => parser.Parse(args));
        }
    }
}
