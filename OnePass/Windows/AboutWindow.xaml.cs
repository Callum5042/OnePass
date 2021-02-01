using OnePass.Infrastructure;
using System.Linq;
using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    [Inject]
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            ShowInTaskbar = false;

            var version = GetType().Assembly.GetName().Version;
            versionTextBlock.Text = $"Version: {version.ToString(3)}";
        }
    }
}
