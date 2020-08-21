namespace OnePass.Services
{
    public interface ISettingsMonitor
    {
        OnePassSettings Current { get; set; }
    }
}