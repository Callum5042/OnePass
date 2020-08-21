using OnePass.Infrastructure;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnePass.Services
{
    [Inject(typeof(IEncryptor))]
    public class Encryptor : IEncryptor
    {
        public async Task EncryptAsync(string file, string password, string data)
        {
            var (Key, IV) = GetKeyAndIv(password);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var msEncrypt = File.Open(file, FileMode.Create);
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            await swEncrypt.WriteAsync(data);
        }

        public async Task<string> DecryptAsync(string file, string password)
        {
            var (Key, IV) = GetKeyAndIv(password);

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            // Create a decryptor to perform the stream transform.
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Create the streams used for decryption.
            using var msDecrypt = File.OpenRead(file);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            var str = await srDecrypt.ReadToEndAsync();
            return str;
        }

        private (byte[] Key, byte[] IV) GetKeyAndIv(string password)
        {
            var sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = Encoding.UTF8.GetBytes(password);
            byte[] rawIV = Encoding.UTF8.GetBytes(password);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            return (hashKey, hashIV);
        }
    }
}
