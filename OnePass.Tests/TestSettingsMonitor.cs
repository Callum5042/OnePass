using OnePass.Services;
using System.Threading.Tasks;

namespace OnePass.Tests
{
    public class TestSettingsMonitor : ISettingsMonitor
    {
        public TestSettingsMonitor(OnePassSettings current)
        {
            Current = current;
        }

        public OnePassSettings Current { get; set; }

        public Task SaveAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
