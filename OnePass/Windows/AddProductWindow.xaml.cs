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
    /// Interaction logic for ProductAuthorWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        private readonly IAddProductHandler _handler;
        private readonly IPasswordGenerator _passwordGenerator;

        public AddProductWindow(IAddProductHandler handler, IPasswordGenerator passwordGenerator)
        {
            InitializeComponent();
            Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            ShowInTaskbar = false;
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));

            NameTextbox.Focus();
        }

        private async void OnClick_AddProduct(object sender, RoutedEventArgs e)
        {
            var isValid = Validate();

            if (isValid)
            {
                var product = new Product()
                {
                    Name = NameTextbox.Text,
                    Login = LoginTextbox.Text,
                    Password = PasswordTextbox.Text
                };

                await _handler.AddProduct(product);
                Close();

                var window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (window?.Content is ViewPage viewPage)
                {
                    viewPage.Products.Add(product);
                }
            }
        }

        private void OnClick_GeneratePassword(object sender, RoutedEventArgs e)
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
                NameValidationMessage.Visibility = Visibility.Collapsed;
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
                LoginValidationMessage.Visibility = Visibility.Collapsed;
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
                PasswordValidationMessage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
