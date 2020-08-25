using OnePass.Infrastructure;
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
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;

            var version = GetType().Assembly.GetName().Version;
            versionTextBlock.Text = $"Version: {version.ToString(3)}";
        }
    }
}
