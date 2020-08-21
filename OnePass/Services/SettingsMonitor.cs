namespace OnePass.Services
{
    public class SettingsMonitor : ISettingsMonitor
    {
        public OnePassSettings Current { get; set; } = new OnePassSettings();
    }
}
