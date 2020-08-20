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
        public Product Product { get; set; }

        public UpdateProductWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            ShowInTaskbar = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnClick_UpdateProduct(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
