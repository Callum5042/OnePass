using OnePass.Services.Interfaces;

namespace OnePass.Tests
{
    public class MockHasher : IHasher
    {
        public string ComputeHashToString(string value)
        {
            return value;
        }
    }
}
