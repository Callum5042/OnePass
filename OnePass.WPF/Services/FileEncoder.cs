using OnePass.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OnePass.WPF.Services
{
    public class FileEncoder
    {
        private const string _fileSignature = ".ONEPASS";
        private const int _fileVersion = 1;

        public FileEncoder()
        {

        }

        public bool Loaded { get; private set; }

        public int Version { get; private set; }

        public IList<Account> Accounts { get; private set; } = new List<Account>();

        public void Load()
        {
            var filename = App.Current.Filename;

            using var file = File.OpenRead(filename);
            using var reader = new BinaryReader(file);

            // Read signature
            var signature = reader.ReadBytes(Encoding.UTF8.GetByteCount(_fileSignature));
            if (Encoding.UTF8.GetString(signature) != _fileSignature)
            {
                throw new InvalidOperationException("Not a valid OnePass file");
            }

            // Read version
            Version = reader.ReadInt32();

            // Read password hash
            var passwordHashLength = reader.ReadInt32();
            var passwordHash = reader.ReadBytes(passwordHashLength);

            // Read salt
            var saltLength = reader.ReadInt32();
            var salt = reader.ReadBytes(saltLength);

            // Read IV
            var ivLength = reader.ReadInt32();
            var iv = reader.ReadBytes(ivLength);

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(App.Current.Password, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);
            aes.IV = iv;

            // Decrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var cryptoReader = new StreamReader(cryptoStream);
            var content = cryptoReader.ReadToEnd();

            var root = JsonSerializer.Deserialize<RootAccount>(content);
            Accounts = root.Accounts;

            Loaded = true;
        }

        public void Save()
        {
            // Generate salt
            var salt = RandomNumberGenerator.GetBytes(8);

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(App.Current.Password, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);

            using var file = File.Create(App.Current.Filename);
            using var writer = new BinaryWriter(file);

            // Write signature
            writer.Write(Encoding.UTF8.GetBytes(_fileSignature));

            // Write version
            writer.Write(_fileVersion);

            // Write password hash
            using (var sha = SHA512.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(App.Current.Password);
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

            // Encrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var cryptoWriter = new StreamWriter(cryptoStream);

            var content = JsonSerializer.Serialize(new RootAccount()
            {
                Accounts = Accounts
            });

            cryptoWriter.Write(content);
        }
    }
}
