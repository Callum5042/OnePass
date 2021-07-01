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
        public Stream Decrypt(Stream stream, string password)
        {
            var (Key, IV) = GetKeyAndIv(password);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
            return cryptoStream;
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
