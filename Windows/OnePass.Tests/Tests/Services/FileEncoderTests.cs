using OnePass.Models;
using OnePass.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public class FileEncoderTests : IDisposable
    {
        private const string Password = "vault-pässword-秘密";
        private const string FixturePassword = "fixture-password";

        // ".ONEPASS" + version + kdf id + iterations + salt length + salt + nonce length + nonce
        private const int SaltOffset = 8 + 4 + 1 + 4 + 4;
        private const int NonceOffset = SaltOffset + 16 + 4;
        private const int HeaderLength = NonceOffset + 12;

        private readonly string _directory;
        private readonly string _filePath;
        private readonly FileEncoder _encoder = new FileEncoder();

        public FileEncoderTests()
        {
            _directory = Path.Combine(Path.GetTempPath(), $"OnePass-{Guid.NewGuid()}");
            Directory.CreateDirectory(_directory);
            _filePath = Path.Combine(_directory, "vault.bin");
        }

        public void Dispose()
        {
            Directory.Delete(_directory, recursive: true);
        }

        [Fact]
        public async Task SaveAsync_WritesVersionTwoHeaderAndRoundTrips()
        {
            var vault = CreateVault();

            await _encoder.SaveAsync("ignored", Password, vault, _filePath);

            var bytes = File.ReadAllBytes(_filePath);
            Assert.Equal(".ONEPASS", Encoding.UTF8.GetString(bytes, 0, 8));
            Assert.Equal(2, BitConverter.ToInt32(bytes, 8));
            Assert.Equal(1, bytes[12]);
            Assert.Equal(600_000, BitConverter.ToInt32(bytes, 13));
            Assert.Equal(16, BitConverter.ToInt32(bytes, 17));
            Assert.Equal(12, BitConverter.ToInt32(bytes, NonceOffset - 4));

            var loaded = await _encoder.LoadAsync("ignored", Password, _filePath);
            AssertVault(loaded, "Ünicode account", "秘密");
        }

        [Fact]
        public async Task SaveAsync_DoesNotStoreThePasswordHash()
        {
            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);

            var bytes = File.ReadAllBytes(_filePath);
            var salt = bytes.AsSpan(SaltOffset, 16).ToArray();
            var legacyHash = SHA512.HashData(Encoding.UTF8.GetBytes(Password).Concat(salt).ToArray());

            Assert.Equal(-1, bytes.AsSpan().IndexOf(legacyHash));
        }

        [Fact]
        public async Task SaveAsync_UsesFreshSaltAndNonce()
        {
            var secondPath = Path.Combine(_directory, "second.bin");

            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);
            await _encoder.SaveAsync("ignored", Password, CreateVault(), secondPath);

            var first = File.ReadAllBytes(_filePath);
            var second = File.ReadAllBytes(secondPath);
            Assert.NotEqual(first.AsSpan(SaltOffset, 16).ToArray(), second.AsSpan(SaltOffset, 16).ToArray());
            Assert.NotEqual(first.AsSpan(NonceOffset, 12).ToArray(), second.AsSpan(NonceOffset, 12).ToArray());
        }

        [Fact]
        public async Task LoadAsync_WrongPassword_ThrowsInvalidPassword()
        {
            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);

            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _encoder.LoadAsync("ignored", "wrong password", _filePath));
        }

        [Theory]
        [InlineData(SaltOffset)]
        [InlineData(NonceOffset)]
        [InlineData(HeaderLength)]
        [InlineData(-1)]
        public async Task LoadAsync_TamperedFile_FailsAuthentication(int offset)
        {
            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);
            var bytes = File.ReadAllBytes(_filePath);
            bytes[offset < 0 ? bytes.Length + offset : offset] ^= 0x01;
            File.WriteAllBytes(_filePath, bytes);

            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _encoder.LoadAsync("ignored", Password, _filePath));
        }

        [Fact]
        public async Task LoadAsync_LoweredIterationCount_FailsAuthentication()
        {
            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);
            var bytes = File.ReadAllBytes(_filePath);
            BitConverter.GetBytes(1000).CopyTo(bytes, 13);
            File.WriteAllBytes(_filePath, bytes);

            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _encoder.LoadAsync("ignored", Password, _filePath));
        }

        [Fact]
        public async Task LoadAsync_TruncatedFile_Throws()
        {
            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);
            File.WriteAllBytes(_filePath, File.ReadAllBytes(_filePath).Take(30).ToArray());

            await Assert.ThrowsAsync<EndOfStreamException>(
                () => _encoder.LoadAsync("ignored", Password, _filePath));
        }

        [Fact]
        public async Task LoadAsync_UnsupportedVersion_Throws()
        {
            await _encoder.SaveAsync("ignored", Password, CreateVault(), _filePath);
            var bytes = File.ReadAllBytes(_filePath);
            BitConverter.GetBytes(3).CopyTo(bytes, 8);
            File.WriteAllBytes(_filePath, bytes);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _encoder.LoadAsync("ignored", Password, _filePath));
        }

        [Fact]
        public async Task LoadAsync_VersionOneFixture_ReturnsVault()
        {
            var loaded = await _encoder.LoadAsync("ignored", FixturePassword, FixturePath("vault-v1.bin"));

            AssertVault(loaded, "Ünicode account", "秘密");
        }

        [Fact]
        public async Task LoadAsync_VersionOneFixtureWrongPassword_ThrowsInvalidPassword()
        {
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _encoder.LoadAsync("ignored", "wrong password", FixturePath("vault-v1.bin")));
        }

        [Fact]
        public async Task LoadAsync_VersionTwoFileWrittenByAndroid_ReturnsVault()
        {
            var loaded = await _encoder.LoadAsync("ignored", Password, FixturePath("vault-v2-android.bin"));

            AssertVault(loaded, "Ünicode account", "秘密");
        }

        [Fact]
        public async Task NeedsUpgrade_VersionOneFile_IsTrueUntilSaved()
        {
            File.Copy(FixturePath("vault-v1.bin"), _filePath);
            Assert.True(_encoder.NeedsUpgrade("ignored", _filePath));

            var vault = await _encoder.LoadAsync("ignored", FixturePassword, _filePath);
            await _encoder.SaveAsync("ignored", FixturePassword, vault, _filePath);

            Assert.False(_encoder.NeedsUpgrade("ignored", _filePath));
            AssertVault(await _encoder.LoadAsync("ignored", FixturePassword, _filePath), "Ünicode account", "秘密");
        }

        private static string FixturePath(string name)
        {
            return Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
        }

        private static OnePassData CreateVault()
        {
            var timestamp = new DateTime(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);
            var vault = new OnePassData();
            vault.Accounts.Add(new Account
            {
                Guid = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                DateCreated = timestamp,
                DateModified = timestamp,
                Name = "Ünicode account",
                Username = "fixture-user",
                EmailAddress = "user@example.com",
                Password = "秘密",
                Favourite = true,
                WebsiteUrl = "https://example.com",
                MfaEnabled = true,
                Notes = "line one\nline two",
                PasswordHistory =
                {
                    new PasswordHistory
                    {
                        Guid = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                        Password = "older",
                        DateTime = timestamp,
                    },
                },
            });
            vault.DeletedAccounts.Add(Guid.Parse("00000000-0000-0000-0000-000000000020"));
            return vault;
        }

        private static void AssertVault(OnePassData vault, string expectedName, string expectedPassword)
        {
            var account = Assert.Single(vault.Accounts);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000010"), account.Guid);
            Assert.Equal(expectedName, account.Name);
            Assert.Equal("fixture-user", account.Username);
            Assert.Equal(expectedPassword, account.Password);
            Assert.True(account.Favourite);
            Assert.True(account.MfaEnabled);
            Assert.Equal("line one\nline two", account.Notes);
            Assert.Equal("older", Assert.Single(account.PasswordHistory).Password);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000020"), Assert.Single(vault.DeletedAccounts));
        }
    }
}
