using System;
using System.IO;
using System.Threading.Tasks;

namespace OnePass.Tests
{
    public class FileCleanupFactory : IDisposable
    {
        private readonly string _filename;

        public FileCleanupFactory(string filename)
        {
            _filename = filename;
        }

        public Task WriteAsync(string value)
        {
            using var file = File.OpenWrite(_filename);
            using var writer = new StreamWriter(file);

            return writer.WriteAsync(value);
        }

        public Task<string> ReadAsync()
        {
            using var file = File.OpenWrite(_filename);
            using var reader = new StreamReader(file);

            return reader.ReadLineAsync();
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
