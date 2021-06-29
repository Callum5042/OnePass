using OnePass.Handlers;
using OnePass.Handlers.Interfaces;
using OnePass.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for UpdateProductWindow.xaml
    /// </summary>
    public partial class UpdateProductWindow : Window
    {
        private readonly IUpdateProductHandler _handler;

        public Product Product { get; set; }

        public UpdateProductWindow(IUpdateProductHandler handler)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            ShowInTaskbar = false;
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NameTextbox.Text = Product.Name;
            LoginTextbox.Text = Product.Login;
            PasswordTextbox.Text = Product.Password;
        }

        private async void OnClick_UpdateProduct(object sender, RoutedEventArgs e)
        {
            var valid = Validate();

            if (valid)
            {
                var result = MessageBox.Show("Update product information?", "Update", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Product.Name = NameTextbox.Text;
                    Product.Login = LoginTextbox.Text;
                    Product.Password = PasswordTextbox.Text;

                    await _handler.UpdateAsync(Product);
                    Close();
                }
            }
        }

        private bool Validate()
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(NameTextbox.Text))
            {
                NameValidationMessage.Content = "'Name' is required.";
                NameValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LoginTextbox.Text))
            {
                LoginValidationMessage.Content = "'Login' is required.";
                LoginValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(PasswordTextbox.Text))
            {
                PasswordValidationMessage.Content = "'Password' is required.";
                PasswordValidationMessage.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void OnTextChanged_NameTextbox(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextbox.Text))
            {
                NameValidationMessage.Content = "'Name' is required.";
                NameValidationMessage.Visibility = Visibility.Visible;
            }
            else
            {
                NameValidationMessage.Content = string.Empty;
                NameValidationMessage.Visibility = Visibility.Hidden;
            }
        }

        private void OnTextChanged_LoginTextbox(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextbox.Text))
            {
                LoginValidationMessage.Content = "'Login' is required.";
                LoginValidationMessage.Visibility = Visibility.Visible;
            }
            else
            {
                LoginValidationMessage.Content = string.Empty;
                LoginValidationMessage.Visibility = Visibility.Hidden;
            }
        }

        private void OnTextChanged_PasswordTextbox(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordTextbox.Text))
            {
                PasswordValidationMessage.Content = "'Password' is required.";
                PasswordValidationMessage.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordValidationMessage.Content = string.Empty;
                PasswordValidationMessage.Visibility = Visibility.Hidden;
            }
        }
    }
}
