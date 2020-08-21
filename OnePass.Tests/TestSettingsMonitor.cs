using OnePass.Services;

namespace OnePass.Tests
{
    public class TestSettingsMonitor : ISettingsMonitor
    {
        public TestSettingsMonitor(OnePassSettings current)
        {
            Current = current;
        }

        public OnePassSettings Current { get; set; }
    }
}
