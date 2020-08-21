using OnePass.Services;

namespace OnePass.Tests.Handlers
{
    public partial class ViewProductHandlerTests
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
}
