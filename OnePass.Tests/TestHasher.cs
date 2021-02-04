using OnePass.Services.Interfaces;

namespace OnePass.Tests
{
    public class TestHasher : IHasher
    {
        public string ComputeHashToString(string value)
        {
            return value;
        }
    }
}
