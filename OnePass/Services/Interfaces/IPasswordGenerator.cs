namespace OnePass.Services.Interfaces
{
    public interface IPasswordGenerator
    {
        string Generate(PasswordGeneratorOptions options);
    }
}