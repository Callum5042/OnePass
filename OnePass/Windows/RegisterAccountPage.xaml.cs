using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for RegisterAccountPage.xaml
    /// </summary>
    public partial class RegisterAccountPage : Page
    {
        public RegisterAccountPage()
        {
            InitializeComponent();
        }

        private void OnClick_NavigateToLogin(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.Content = app.GetService<LoginPage>();
        }

        private void OnClick_CreateAccount(object sender, RoutedEventArgs e)
        {

        }
    }
}
