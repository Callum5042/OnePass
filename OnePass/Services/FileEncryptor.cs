using OnePass.Infrastructure;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnePass.Services
{
    [Inject]
    public class FileEncryptor : IFileEncryptor
    {
        public async Task EncryptAsync(Stream input, Stream output, string password)
        {
            // Encryption Key
            var (Key, IV) = GetKeyAndIv("super");

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Padding = PaddingMode.PKCS7;

            // Encrypt file
            using var decryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            await cryptoStream.CopyToAsync(output);
        }

        public async Task DecryptAsync(Stream input, Stream output, string password)
        {
            // Encrypted key
            var (Key, IV) = GetKeyAndIv("super");

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Padding = PaddingMode.PKCS7;

            // Decrypt file
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            await cryptoStream.CopyToAsync(output);
        }

        private static (byte[] Key, byte[] IV) GetKeyAndIv(string password)
        {
            var sha2 = new SHA256CryptoServiceProvider();

            var rawKey = Encoding.UTF8.GetBytes(password);
            var rawIV = Encoding.UTF8.GetBytes(password);

            var hashKey = sha2.ComputeHash(rawKey);
            var hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashKey, 16);
            Array.Resize(ref hashIV, 16);
            return (hashKey, hashIV);
        }
    }
}
