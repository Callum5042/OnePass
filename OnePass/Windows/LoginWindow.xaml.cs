using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var app = Application.Current as App;
            Content = app.GetService<LoginPage>();
        }
    }
}
