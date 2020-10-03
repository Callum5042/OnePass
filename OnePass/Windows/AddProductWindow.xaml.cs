using OnePass.Handlers;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ProductAuthorWindow.xaml
    /// </summary>
    [Inject]
    public partial class AddProductWindow : Window
    {
        private readonly IAddProductHandler _handler;
        private readonly IPasswordGenerator _passwordGenerator;

        public AddProductWindow(IAddProductHandler handler, IPasswordGenerator passwordGenerator)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
        }

        private async void OnClick_AddProduct(object sender, RoutedEventArgs e)
        {
            var product = new Product()
            {
                Name = NameTextbox.Text,
                Login = LoginTextbox.Text,
                Password = PasswordTextbox.Text
            };

            await _handler.AddProduct(product);
            Close();

            var view = (Application.Current.MainWindow.Content as ViewPage);
            view.Products.Add(product);
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
    }
}
