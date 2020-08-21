using OnePass.Handlers;
using OnePass.Infrastructure;
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

        public AddProductWindow(IAddProductHandler handler)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
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
    }
}
