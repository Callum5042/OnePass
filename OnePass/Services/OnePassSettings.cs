namespace OnePass.Services
{
    public class OnePassSettings
    {
        public string Test { get; set; }

        public bool IsLoggedIn { get; set; } = false;

        public string MasterPassword { get; set; } = "password123";

        public string FileName { get; set; } = "data.bin";
    }
}
