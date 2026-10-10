using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public sealed class FileEncoderTests : IDisposable
    {
        private const string Password = "test vault password";
        private readonly string directory = Path.Combine(Path.GetTempPath(), "OnePass-save-tests-" + Guid.NewGuid());
        private string Vault => Path.Combine(directory, "vault.bin");

        public FileEncoderTests() => Directory.CreateDirectory(directory);
        public void Dispose() => Directory.Delete(directory, recursive: true);
        private static OnePassData Data(string name) => new OnePassData
        {
            Accounts = new List<Account> { new Account { Guid = Guid.NewGuid(), Name = name, Password = "secret" } }
        };

        private async Task AssertDecrypts(string path, string name)
        {
            var encoder = new FileEncoder(new VaultFileStore());
            Assert.True(encoder.Verify("ignored", Password, path));
            Assert.Equal(name, Assert.Single((await encoder.LoadAsync("ignored", Password, path)).Accounts).Name);
        }

        [Fact]
        public async Task Replacement_PreservesPreviousEncryptedVaultAndAllowsRecovery()
        {
            var encoder = new FileEncoder(new VaultFileStore());
            await encoder.SaveAsync("ignored", Password, Data("first"), Vault);
            Assert.False(File.Exists(Vault + ".bak"));
            await encoder.SaveAsync("ignored", Password, Data("second"), Vault);
            await AssertDecrypts(Vault, "second");
            await AssertDecrypts(Vault + ".bak", "first");
            await encoder.SaveAsync("ignored", Password, Data("third"), Vault);
            await AssertDecrypts(Vault, "third");
            await AssertDecrypts(Vault + ".bak", "second");
            var restored = Path.Combine(directory, "restored.bin");
            File.Copy(Vault + ".bak", restored);
            await AssertDecrypts(restored, "second");
            Assert.False(File.Exists(Vault + ".tmp"));
        }

        [Theory]
        [InlineData("create")]
        [InlineData("write")]
        [InlineData("flush")]
        [InlineData("close")]
        [InlineData("commit")]
        public async Task FailedSave_PreservesOriginalAndExistingBackup(string stage)
        {
            var encoder = new FileEncoder(new VaultFileStore());
            await encoder.SaveAsync("ignored", Password, Data("backup"), Vault);
            await encoder.SaveAsync("ignored", Password, Data("original"), Vault);
            var original = File.ReadAllBytes(Vault);
            var backup = File.ReadAllBytes(Vault + ".bak");

            await Assert.ThrowsAsync<IOException>(() =>
                new FileEncoder(new FailingStore(stage)).SaveAsync("ignored", Password, Data("new"), Vault));

            Assert.Equal(original, File.ReadAllBytes(Vault));
            Assert.Equal(backup, File.ReadAllBytes(Vault + ".bak"));
            await AssertDecrypts(Vault, "original");
            Assert.False(File.Exists(Vault + ".tmp"));
        }

        [Theory]
        [InlineData("create")]
        [InlineData("write")]
        [InlineData("flush")]
        [InlineData("close")]
        [InlineData("commit")]
        public async Task FailedInitialSave_DoesNotPublishVault(string stage)
        {
            await Assert.ThrowsAsync<IOException>(() =>
                new FileEncoder(new FailingStore(stage)).SaveAsync("ignored", Password, Data("new"), Vault));
            Assert.False(File.Exists(Vault));
            Assert.False(File.Exists(Vault + ".tmp"));
        }

        [Fact]
        public async Task SerializationFailure_PreservesOriginal()
        {
            var encoder = new FileEncoder(new VaultFileStore());
            await encoder.SaveAsync("ignored", Password, Data("original"), Vault);
            var invalid = new OnePassData { Accounts = new ThrowingAccounts() };
            await Assert.ThrowsAsync<InvalidOperationException>(() => encoder.SaveAsync("ignored", Password, invalid, Vault));
            await AssertDecrypts(Vault, "original");
            Assert.False(File.Exists(Vault + ".tmp"));
        }

        [Fact]
        public async Task InterruptedAfterCommit_LeavesCompleteNewVaultAndBackup()
        {
            await new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("old"), Vault);
            await Assert.ThrowsAsync<IOException>(() =>
                new FileEncoder(new FailingStore("afterCommit")).SaveAsync("ignored", Password, Data("new"), Vault));
            await AssertDecrypts(Vault, "new");
            await AssertDecrypts(Vault + ".bak", "old");
        }

        [Fact]
        public async Task CleanupFailure_DoesNotReportCommittedSaveAsFailed()
        {
            await new FileEncoder(new FailingStore("cleanup")).SaveAsync("ignored", Password, Data("new"), Vault);
            await AssertDecrypts(Vault, "new");
        }

        [Fact]
        public async Task AbandonedTemporaryFile_IsReplacedOnNextSave()
        {
            await new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("old"), Vault);
            File.WriteAllBytes(Vault + ".tmp", new byte[] { 1, 2, 3 });
            await new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("new"), Vault);
            await AssertDecrypts(Vault, "new");
            await AssertDecrypts(Vault + ".bak", "old");
            Assert.False(File.Exists(Vault + ".tmp"));
        }

        [Fact]
        public async Task OverlappingSave_IsRejectedWithoutTouchingVaultOrActiveTemporaryFile()
        {
            await new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("old"), Vault);
            using (new VaultFileStore().AcquireLock(Vault))
            {
                File.WriteAllText(Vault + ".tmp", "active save");
                await Assert.ThrowsAsync<IOException>(() => new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("new"), Vault));
                Assert.Equal("active save", File.ReadAllText(Vault + ".tmp"));
                await AssertDecrypts(Vault, "old");
            }
            await new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("new"), Vault);
            await AssertDecrypts(Vault, "new");
        }

        [Fact]
        public async Task RealReplacementFailure_PreservesOriginal()
        {
            await new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("old"), Vault);
            using (File.Open(Vault, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await Assert.ThrowsAsync<IOException>(() => new FileEncoder(new VaultFileStore()).SaveAsync("ignored", Password, Data("new"), Vault));
                await AssertDecrypts(Vault, "old");
            }
            Assert.False(File.Exists(Vault + ".tmp"));
        }

        private sealed class ThrowingAccounts : Collection<Account>, IEnumerable<Account>
        {
            IEnumerator<Account> IEnumerable<Account>.GetEnumerator() => throw new InvalidOperationException("Injected serialization failure");
        }

        private sealed class FailingStore(string stage) : IVaultFileStore
        {
            private readonly VaultFileStore real = new VaultFileStore();
            public Stream AcquireLock(string path) => real.AcquireLock(path);
            public Stream CreateTemporary(string path)
            {
                Fail("create");
                var stream = real.CreateTemporary(path);
                return stage == "write" || stage == "close" ? new FailingStream(stream, stage) : stream;
            }
            public void FlushToDisk(Stream stream)
            {
                Fail("flush");
                // The close-failure wrapper still needs to flush the actual file before closing.
                real.FlushToDisk(stream is FailingStream wrapper ? wrapper.Inner : stream);
            }
            public void Commit(string temporaryPath, string destinationPath)
            {
                Fail("commit");
                real.Commit(temporaryPath, destinationPath);
                Fail("afterCommit");
            }
            public void DeleteTemporary(string path) { Fail("cleanup"); real.DeleteTemporary(path); }
            private void Fail(string point) { if (stage == point) throw new IOException("Injected " + point + " failure"); }
        }

        private sealed class FailingStream(Stream inner, string stage) : Stream
        {
            public Stream Inner => inner;
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => inner.Length;
            public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
            public override void Flush() => inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count)
            {
                if (stage == "write" && inner.Position + count > 130)
                {
                    inner.Write(buffer, offset, Math.Min(count, 3));
                    throw new IOException("Injected partial encrypted write failure");
                }
                inner.Write(buffer, offset, count);
            }
            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var bytes = buffer.ToArray();
                Write(bytes, 0, bytes.Length);
                return ValueTask.CompletedTask;
            }
            protected override void Dispose(bool disposing)
            {
                if (disposing) inner.Dispose();
                base.Dispose(disposing);
                if (disposing && stage == "close") throw new IOException("Injected close failure");
            }
        }
    }
}
