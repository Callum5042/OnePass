using Newtonsoft.Json;
using OnePass.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OnePass
{
    public class RootAccount
    {
        public IList<AccountV2> Accounts { get; set; }
    }

    public class FileCreator
    {
        private const string _fileSignature = ".ONEPASS";
        private const int _fileVersion = 1;

        public void Create(string filename, string password)
        {
            // Generate salt
            var salt = new byte[8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(password, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);

            using var file = File.Create($"{filename}.bin");
            using var writer = new BinaryWriter(file);

            // Write signature
            writer.Write(Encoding.UTF8.GetBytes(_fileSignature));

            // Write version
            writer.Write(_fileVersion);

            // Write password hash
            using (var sha = SHA512.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var bytes = passwordBytes.Concat(salt).ToArray();
                var passwordHash = sha.ComputeHash(bytes);

                writer.Write(passwordHash.Length);
                writer.Write(passwordHash);
            }

            // Write salt
            writer.Write(salt.Length);
            writer.Write(salt);

            // Write IV
            writer.Write(aes.IV.Length);
            writer.Write(aes.IV);

            // Content
            var root = new RootAccount();
            var content = JsonConvert.SerializeObject(root);

            // Encrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var cryptoWriter = new StreamWriter(cryptoStream);
            cryptoWriter.Write(content);
        }
    }
}
