using OnePass.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OnePass.Tests
{
    public class EncryptorCleanupFactory : IDisposable
    {
        private readonly string _filename;

        public EncryptorCleanupFactory(string filename)
        {
            _filename = filename;
        }

        public Task Encrypt(string password, string json)
        {
            var encryptor = new Encryptor();
            return encryptor.EncryptAsync(_filename, password, json);
        }

        public void Dispose()
        {
            if (File.Exists(_filename))
            {
                File.Delete(_filename);
            }

            GC.SuppressFinalize(this);
        }
    }
}
