using OnePass.Handlers;
using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    [Inject]
    public partial class ChangePasswordWindow : Window
    {
        private readonly IChangePasswordHandler _changePasswordHandler;

        public ChangePasswordWindow(IChangePasswordHandler changePasswordHandler)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;

            _changePasswordHandler = changePasswordHandler ?? throw new ArgumentNullException(nameof(changePasswordHandler));
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var oldPassword = oldPassTextbox.Password;
            var newPassword = newPassTextbox.Password;
            var repeatPassword = repeatPassTextbox.Password;

            if (newPassword != repeatPassword)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            var message = await _changePasswordHandler.ChangePassword(oldPassword, newPassword);
            MessageBox.Show(message);
        }
    }
}
