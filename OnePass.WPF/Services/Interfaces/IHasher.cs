namespace OnePass.Services.Interfaces
{
    public interface IHasher
    {
        string ComputeHashToString(string value);
    }
}