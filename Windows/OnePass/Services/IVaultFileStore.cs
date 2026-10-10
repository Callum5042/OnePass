using System.IO;

namespace OnePass.Services
{
    public interface IVaultFileStore
    {
        Stream AcquireLock(string path);
        Stream CreateTemporary(string path);
        void FlushToDisk(Stream stream);
        void Commit(string temporaryPath, string destinationPath);
        void DeleteTemporary(string path);
    }
}
