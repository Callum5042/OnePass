using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Models;
using OnePass.WPF.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.WPF.Tests.Tests
{
    public class VaultFilePathModelTests
    {
        [Fact]
        public async Task ContentLoad_InitialDecryptedVault_DoesNotReadFileAgain()
        {
            var encoder = new RecordingFileEncoder();
            var session = CreateSession();
            session.InitialVaultData = new OnePassData();
            var model = new ContentModel(encoder, session);

            await model.LoadAsync();

            Assert.Equal(0, encoder.LoadCount);
            Assert.Null(session.InitialVaultData);
        }

        [Fact]
        public async Task ContentLoad_UsesSelectedFilePath()
        {
            var encoder = new RecordingFileEncoder();
            var session = CreateSession();
            var model = new ContentModel(encoder, session);

            await model.LoadAsync();

            Assert.Equal(session.FilePath, encoder.LastLoadPath);
        }

        [Fact]
        public async Task ContentRemove_LoadsAndSavesSelectedFilePath()
        {
            var accountGuid = Guid.NewGuid();
            var encoder = new RecordingFileEncoder
            {
                Data = new OnePassData
                {
                    Accounts =
                    {
                        new Account { Guid = accountGuid, Name = "Example" }
                    }
                }
            };
            var session = CreateSession();
            var model = new ContentModel(encoder, session);

            await model.RemoveAsync(new AccountListModel { Guid = accountGuid });

            Assert.Equal(session.FilePath, encoder.LastLoadPath);
            Assert.Equal(session.FilePath, encoder.LastSavePath);
            Assert.Contains(accountGuid, encoder.Data.DeletedAccounts);
            Assert.Empty(encoder.Data.Accounts);
        }

        [Fact]
        public async Task AccountLoad_UsesSelectedFilePath()
        {
            var encoder = new RecordingFileEncoder();
            var session = CreateSession();
            var model = new AccountModel(encoder, session);

            await model.LoadAsync();

            Assert.Equal(session.FilePath, encoder.LastLoadPath);
        }

        private static UserData CreateSession()
        {
            return new UserData
            {
                Username = "vault",
                FilePath = @"D:\vaults\selected.bin",
                Password = "Password123456789"
            };
        }

        private sealed class RecordingFileEncoder : IFileEncoder
        {
            public OnePassData Data { get; set; } = new OnePassData();

            public int LoadCount { get; private set; }

            public string LastLoadPath { get; private set; }

            public string LastSavePath { get; private set; }

            public Task<OnePassData> LoadAsync(string username, string password, string path = null)
            {
                LoadCount++;
                LastLoadPath = path;
                return Task.FromResult(Data);
            }

            public Task SaveAsync(string username, string password, OnePassData rootAccount, string path = null)
            {
                Data = rootAccount;
                LastSavePath = path;
                return Task.CompletedTask;
            }

            public bool Verify(string username, string password, string path = null)
            {
                return true;
            }
        }
    }
}
