using OnePass.Handlers;
using OnePass.Handlers.Interfaces;
using OnePass.Models;
using OnePass.Services;
using OnePass.Services.Interfaces;
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
        private readonly IPasswordGenerator _passwordGenerator;

        private bool IsNewPasswordGenerated { get; set; }

        private bool IsContentModified { get; set; }

        public Product Product { get; set; }

        public UpdateProductWindow(IUpdateProductHandler handler, IPasswordGenerator passwordGenerator)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            ShowInTaskbar = false;

            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NameTextbox.Text = Product.Name;
            LoginTextbox.Text = Product.Login;
            PasswordTextbox.Text = Product.Password;

            // Set to false once content is loaded
            IsContentModified = false;
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
                    IsContentModified = false;
                    Close();
                }
            }
        }

        private void OnClick_GeneratePasswordButton(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordTextbox.Text) || IsNewPasswordGenerated)
            {
                var password = _passwordGenerator.Generate(new PasswordGeneratorOptions()
                {
                    MinLength = 10,
                    MaxLength = 14,
                    Uppercase = true,
                    Lowercase = true,
                    Numbers = true,
                    Symbols = true,
                    SymbolAmount = 1
                });

                PasswordTextbox.Text = password;
                IsNewPasswordGenerated = true;
            }
            else
            {
                MessageBox.Show("Cannot generate password when the textbox is filled.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            IsContentModified = true;
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
            IsContentModified = true;
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
            IsContentModified = true;
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

        private void OnClosing_Window(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsContentModified)
            {
                var msg = MessageBox.Show("Exit without saving changes?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (msg == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
