using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Models;
using OnePass.WPF.Services;
using System;
using System.IO;
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

        [Fact]
        public async Task ContentRemove_SaveFailure_KeepsAccountVisibleAndDataUnchanged()
        {
            var accountGuid = Guid.NewGuid();
            var original = new OnePassData
            {
                Accounts = { new Account { Guid = accountGuid, Name = "Example" } }
            };
            var encoder = new RecordingFileEncoder { Data = original, FailSave = true };
            var model = new ContentModel(encoder, CreateSession());
            await model.LoadAsync();
            var visible = Assert.Single(model.Accounts);

            await Assert.ThrowsAsync<IOException>(() => model.RemoveAsync(visible));

            Assert.Same(visible, Assert.Single(model.Accounts));
            Assert.Same(visible, Assert.Single(model.AccountListModel));
            Assert.Same(visible, model.SelectedAccount);
            Assert.Single(original.Accounts);
            Assert.Empty(original.DeletedAccounts);
        }

        [Fact]
        public async Task ContentRemove_PublishesOnlyAfterSaveCompletes()
        {
            var accountGuid = Guid.NewGuid();
            var saveCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var encoder = new RecordingFileEncoder
            {
                Data = new OnePassData { Accounts = { new Account { Guid = accountGuid } } },
                SaveCompletion = saveCompletion.Task
            };
            var model = new ContentModel(encoder, CreateSession());
            await model.LoadAsync();
            var removal = model.RemoveAsync(Assert.Single(model.Accounts));
            Assert.Single(model.Accounts);
            Assert.Single(model.AccountListModel);

            saveCompletion.SetResult();
            await removal;
            Assert.Empty(model.Accounts);
            Assert.Empty(model.AccountListModel);
        }

        [Fact]
        public async Task AccountUpdate_SaveFailure_DoesNotPublishEditedData()
        {
            var originalAccount = new Account { Guid = Guid.NewGuid(), Name = "Original", Password = "unchanged" };
            var original = new OnePassData { Accounts = { originalAccount } };
            var encoder = new RecordingFileEncoder { Data = original, FailSave = true };
            var model = new AccountModel(encoder, CreateSession())
            {
                OnePassData = original,
                Guid = originalAccount.Guid,
                Name = "Edited",
                Password = originalAccount.Password
            };

            await Assert.ThrowsAsync<IOException>(() => model.UpdateAccountAsync());

            Assert.Same(original, model.OnePassData);
            Assert.Equal("Original", originalAccount.Name);
            Assert.Null(originalAccount.DateModified);
        }

        [Fact]
        public async Task VaultCreation_SaveFailure_DoesNotReportSuccess()
        {
            var encoder = new RecordingFileEncoder { FailSave = true };
            var model = new LoginModel(encoder);
            await Assert.ThrowsAsync<IOException>(() => model.CreateAccountAsync(CreateSession().FilePath, "password"));
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

            public bool FailSave { get; set; }

            public Task SaveCompletion { get; set; } = Task.CompletedTask;

            public Task<OnePassData> LoadAsync(string username, string password, string path = null)
            {
                LoadCount++;
                LastLoadPath = path;
                return Task.FromResult(Data);
            }

            public async Task SaveAsync(string username, string password, OnePassData rootAccount, string path = null)
            {
                LastSavePath = path;
                if (FailSave)
                {
                    throw new IOException("Injected save failure");
                }
                await SaveCompletion;
                Data = rootAccount;
            }

            public bool Verify(string username, string password, string path = null)
            {
                return true;
            }
        }
    }
}
