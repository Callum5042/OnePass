using OnePass.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Services
{
    /// <summary>
    /// Reads and writes OnePass vault files.
    ///
    /// Version 2 layout (all integers little-endian):
    ///   ".ONEPASS" | int32 version | byte kdf id | int32 iterations
    ///   | int32 salt length | salt | int32 nonce length | nonce
    ///   | AES-256-GCM ciphertext | 16-byte tag
    ///
    /// Everything before the ciphertext is authenticated as associated data. No
    /// password hash is stored: a wrong password is detected by the tag failing.
    /// Version 1 files are still readable and are rewritten as version 2 on save.
    /// </summary>
    public class FileEncoder : IFileEncoder
    {
        private const string _fileSignature = ".ONEPASS";
        private const int _legacyFileVersion = 1;
        private const int _fileVersion = 2;

        private const byte _kdfPbkdf2Sha256 = 1;
        private const int _pbkdf2Iterations = 600_000;
        private const int _maxPbkdf2Iterations = 10_000_000;
        private const int _keyLength = 32;
        private const int _saltLength = 16;
        private const int _maxSaltLength = 1024;
        private const int _nonceLength = 12;
        private const int _tagLength = 16;

        private const int _legacyPbkdf2Iterations = 1000;
        private const int _legacyKeyLength = 16;
        private const int _legacyIvLength = 16;
        private const int _legacyMaxHashLength = 128;

        public async Task<OnePassData> LoadAsync(string username, string password, string filename = null)
        {
            filename ??= $"{username}.bin";

            var bytes = await File.ReadAllBytesAsync(filename);

            // Key derivation is deliberately slow, so keep it off the calling (UI) thread.
            return await Task.Run(() => Decode(bytes, password));
        }

        public async Task SaveAsync(string username, string password, OnePassData rootAccount, string filename = null)
        {
            filename ??= $"{username}.bin";

            var bytes = await Task.Run(() => Encode(password, rootAccount));
            await File.WriteAllBytesAsync(filename, bytes);
        }

        public bool NeedsUpgrade(string username, string filename = null)
        {
            filename ??= $"{username}.bin";

            using (var file = File.OpenRead(filename))
            using (var reader = new BinaryReader(file))
            {
                return ReadVersion(reader) < _fileVersion;
            }
        }

        private static OnePassData Decode(byte[] bytes, string password)
        {
            using (var stream = new MemoryStream(bytes, writable: false))
            using (var reader = new BinaryReader(stream))
            {
                var version = ReadVersion(reader);
                switch (version)
                {
                    case _legacyFileVersion:
                        return DecodeLegacy(reader, bytes, password);
                    case _fileVersion:
                        return DecodeCurrent(reader, bytes, password);
                    default:
                        throw new InvalidOperationException($"Unsupported OnePass file version: {version}");
                }
            }
        }

        private static OnePassData DecodeCurrent(BinaryReader reader, byte[] bytes, string password)
        {
            var kdf = reader.ReadByte();
            if (kdf != _kdfPbkdf2Sha256)
            {
                throw new InvalidOperationException($"Unsupported OnePass key derivation: {kdf}");
            }

            var iterations = reader.ReadInt32();
            if (iterations < 1 || iterations > _maxPbkdf2Iterations)
            {
                throw new InvalidOperationException("Invalid OnePass key derivation settings");
            }

            var salt = ReadLengthPrefixed(reader, _maxSaltLength);
            var nonce = ReadLengthPrefixed(reader, _nonceLength);
            if (nonce.Length != _nonceLength)
            {
                throw new InvalidOperationException("Invalid OnePass nonce length");
            }

            var headerLength = (int)reader.BaseStream.Position;
            var ciphertextLength = bytes.Length - headerLength - _tagLength;
            if (ciphertextLength < 0)
            {
                throw new EndOfStreamException("Unexpected end of OnePass file");
            }

            var header = bytes.AsSpan(0, headerLength);
            var ciphertext = bytes.AsSpan(headerLength, ciphertextLength);
            var tag = bytes.AsSpan(headerLength + ciphertextLength, _tagLength);

            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, _keyLength);
            var plaintext = new byte[ciphertextLength];
            try
            {
                using (var aes = new AesGcm(key, _tagLength))
                {
                    try
                    {
                        aes.Decrypt(nonce, ciphertext, tag, plaintext, header);
                    }
                    catch (AuthenticationTagMismatchException)
                    {
                        throw new InvalidPasswordException();
                    }
                }

                return JsonSerializer.Deserialize<OnePassData>(plaintext);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }

        /// <summary>
        /// Version 1: SHA-512(password + salt) stored in the header, PBKDF2-HMAC-SHA1
        /// with 1,000 iterations and AES-128-CBC. Read-only, kept so old vaults still open.
        /// </summary>
        private static OnePassData DecodeLegacy(BinaryReader reader, byte[] bytes, string password)
        {
            var passwordHash = ReadLengthPrefixed(reader, _legacyMaxHashLength);
            var salt = ReadLengthPrefixed(reader, _maxSaltLength);
            var iv = ReadLengthPrefixed(reader, _legacyIvLength);
            if (iv.Length != _legacyIvLength)
            {
                throw new InvalidOperationException("Invalid OnePass IV length");
            }

            var expectedHash = SHA512.HashData(Encoding.UTF8.GetBytes(password).Concat(salt).ToArray());
            if (!CryptographicOperations.FixedTimeEquals(expectedHash, passwordHash))
            {
                throw new InvalidPasswordException();
            }

            var payloadOffset = (int)reader.BaseStream.Position;
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, _legacyPbkdf2Iterations, HashAlgorithmName.SHA1, _legacyKeyLength);
            byte[] plaintext = null;
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    plaintext = aes.DecryptCbc(bytes.AsSpan(payloadOffset), iv);
                }

                return JsonSerializer.Deserialize<OnePassData>(plaintext);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
                if (plaintext != null)
                {
                    CryptographicOperations.ZeroMemory(plaintext);
                }
            }
        }

        private static byte[] Encode(string password, OnePassData data)
        {
            var salt = RandomNumberGenerator.GetBytes(_saltLength);
            var nonce = RandomNumberGenerator.GetBytes(_nonceLength);

            using (var output = new MemoryStream())
            using (var writer = new BinaryWriter(output))
            {
                writer.Write(Encoding.UTF8.GetBytes(_fileSignature));
                writer.Write(_fileVersion);
                writer.Write(_kdfPbkdf2Sha256);
                writer.Write(_pbkdf2Iterations);
                writer.Write(salt.Length);
                writer.Write(salt);
                writer.Write(nonce.Length);
                writer.Write(nonce);
                writer.Flush();

                // The header is authenticated so its settings cannot be altered undetected.
                var header = output.ToArray();

                var plaintext = JsonSerializer.SerializeToUtf8Bytes(data);
                var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, _pbkdf2Iterations, HashAlgorithmName.SHA256, _keyLength);
                var ciphertext = new byte[plaintext.Length];
                var tag = new byte[_tagLength];
                try
                {
                    using (var aes = new AesGcm(key, _tagLength))
                    {
                        aes.Encrypt(nonce, plaintext, ciphertext, tag, header);
                    }
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(key);
                    CryptographicOperations.ZeroMemory(plaintext);
                }

                writer.Write(ciphertext);
                writer.Write(tag);
                writer.Flush();

                return output.ToArray();
            }
        }

        private static int ReadVersion(BinaryReader reader)
        {
            var signature = reader.ReadBytes(Encoding.UTF8.GetByteCount(_fileSignature));
            if (Encoding.UTF8.GetString(signature) != _fileSignature)
            {
                throw new InvalidOperationException("Not a valid OnePass file");
            }

            return reader.ReadInt32();
        }

        private static byte[] ReadLengthPrefixed(BinaryReader reader, int maxLength)
        {
            var length = reader.ReadInt32();
            if (length < 1 || length > maxLength)
            {
                throw new InvalidOperationException($"Invalid OnePass field length: {length}");
            }

            var value = reader.ReadBytes(length);
            if (value.Length != length)
            {
                throw new EndOfStreamException("Unexpected end of OnePass file");
            }

            return value;
        }
    }
}
