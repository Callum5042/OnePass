using OnePass.Infrastructure;
using OnePass.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OnePass.Services
{
    [Inject(typeof(IHasher))]
    public class Hasher : IHasher
    {
        public static byte[] ComputeHash(string value)
        {
            var hasher = SHA256.Create();
            var saltedPassword = value;
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

            return hash;
        }

        public string ComputeHashToString(string value)
        {
            var hash = ComputeHash(value);
            return Encoding.UTF8.GetString(hash);
        }
    }
}
