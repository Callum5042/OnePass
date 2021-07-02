using OnePass.Infrastructure;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OnePass.Services
{
    [Inject]
    public class FileEncryptor : IFileEncryptor
    {
        public void Encrypt(Stream input, Stream output, string password)
        {
            // Encryption Key
            var (Key, IV) = GetKeyAndIv("super");

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            // Encrypt file
            using var decryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            cryptoStream.CopyTo(output);
        }

        public void Decrypt(Stream input, Stream output, string password)
        {
            // Encrypted key
            var (Key, IV) = GetKeyAndIv("super");

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            // Decrypt file
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            cryptoStream.CopyTo(output);
        }

        private static (byte[] Key, byte[] IV) GetKeyAndIv(string password)
        {
            var sha2 = new SHA256CryptoServiceProvider();

            var rawKey = Encoding.UTF8.GetBytes(password);
            var rawIV = Encoding.UTF8.GetBytes(password);

            var hashKey = sha2.ComputeHash(rawKey);
            var hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);
            return (hashKey, hashIV);
        }
    }
}
