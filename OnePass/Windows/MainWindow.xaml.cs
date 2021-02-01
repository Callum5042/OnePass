using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var app = Application.Current as App;
            Content = app.GetService<ViewPage>();
        }
    }
}
