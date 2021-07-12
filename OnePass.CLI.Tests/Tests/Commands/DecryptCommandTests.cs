using OnePass.CLI.Commands;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.CLI.Tests.Tests.Commands
{
    public class DecryptCommandTests : TestSetup
    {
        [Fact]
        public async Task Execute()
        {
            // Arrange
            var arguments = new Arguments()
            {
                CommandType = CommandType.Decrypt,
                File = "test.bin"
            };

            // Act
            var fileEncryptor = new MockEncryptor();
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { arguments.File, new MockFileData("Testing") }
            });

            var command = new DecryptCommand(mockFileSystem, fileEncryptor);
            await command.ExecuteAsync(arguments);

            // Assert
            Assert.Equal(2, mockFileSystem.AllFiles.Count());
        }
    }
}
