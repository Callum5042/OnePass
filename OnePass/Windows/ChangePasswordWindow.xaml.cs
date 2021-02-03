using OnePass.Handlers.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private readonly IChangePasswordHandler _changePasswordHandler;

        public ChangePasswordWindow(IChangePasswordHandler changePasswordHandler)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
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
