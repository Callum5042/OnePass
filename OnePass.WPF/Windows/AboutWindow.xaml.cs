using OnePass.WPF.Infrastructure;
using System.Reflection;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public static string Version => $"Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}
