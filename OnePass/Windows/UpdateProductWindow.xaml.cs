using OnePass.Handlers;
using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Windows;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for UpdateProductWindow.xaml
    /// </summary>
    [Inject]
    public partial class UpdateProductWindow : Window
    {
        private readonly IUpdateProductHandler _handler;

        public Product Product { get; set; }

        public UpdateProductWindow(IUpdateProductHandler handler)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
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
            Product.Name = NameTextbox.Text;
            Product.Login = LoginTextbox.Text;
            Product.Password = PasswordTextbox.Text;

            await _handler.UpdateAsync(Product.Id, Product);
            Close();
        }
    }
}
