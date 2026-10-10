using System.IO;

namespace OnePass.Services
{
    public sealed class VaultFileStore : IVaultFileStore
    {
        // Keep this file: deleting it after releasing the handle would race another saver.
        public Stream AcquireLock(string path) =>
            new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

        public Stream CreateTemporary(string path) =>
            new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                4096, FileOptions.Asynchronous | FileOptions.WriteThrough);

        public void FlushToDisk(Stream stream) => ((FileStream)stream).Flush(flushToDisk: true);

        public void Commit(string temporaryPath, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                // Never fall back to deleting or overwriting the live vault if replacement fails.
                File.Replace(temporaryPath, destinationPath, destinationPath + ".bak");
            }
            else
            {
                File.Move(temporaryPath, destinationPath);
            }
        }

        public void DeleteTemporary(string path) => File.Delete(path);
    }
}
