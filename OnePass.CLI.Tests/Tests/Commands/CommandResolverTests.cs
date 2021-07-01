using OnePass.CLI.Commands;
using System;
using Xunit;

namespace OnePass.CLI.Tests.Tests.Commands
{
    public class CommandResolverTests : TestSetup
    {
        [Theory]
        [InlineData(CommandType.Encrypt, typeof(EncryptCommand))]
        [InlineData(CommandType.Decrypt, typeof(DecryptCommand))]
        [InlineData(CommandType.Help, typeof(HelpCommand))]
        public void Resolve_MatchCommandType_ReturnsCommand(CommandType commandType, Type type)
        {
            // Act
            var resolver = GetService<CommandResolver>();
            var command = resolver.Resolve(commandType);

            // Assert
            Assert.IsType(type, command);
        }
    }
}
