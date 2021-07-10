namespace OnePass.Services
{
    public interface IPasswordGenerator
    {
        string Generate(PasswordGeneratorOptions options);
    }
}