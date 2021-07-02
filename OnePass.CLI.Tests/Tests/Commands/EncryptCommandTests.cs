using OnePass.CLI.Commands;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace OnePass.CLI.Tests.Tests.Commands
{
    public class EncryptCommandTests : TestSetup
    {
        [Fact]
        public void Execute()
        {
            // Arrange
            var arguments = new Arguments()
            {
                CommandType = CommandType.Encrypt,
                File = "test.bin"
            };

            // Act
            var fileEncryptor = new MockEncryptor();
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { arguments.File, new MockFileData("Testing") }
            });

            var command = new EncryptCommand(mockFileSystem, fileEncryptor);
            command.Execute(arguments);

            // Assert
            Assert.Equal(2, mockFileSystem.AllFiles.Count());
        }
    }
}
