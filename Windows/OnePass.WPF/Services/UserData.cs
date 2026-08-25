using OnePass.Models;

namespace OnePass.WPF.Services
{
    public class UserData
    {
        public string Username { get; set; }

        public string FilePath { get; set; }

        public string Password { get; set; }

        public OnePassData InitialVaultData { get; set; }
    }
}
