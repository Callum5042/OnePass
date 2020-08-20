using OnePass.Handlers;
using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ViewPage.xaml
    /// </summary>
    [Inject]
    public partial class ViewPage : Page
    {
        private readonly ViewPageHandler _handler;

        public ViewPage(ViewPageHandler handler)
        {
            InitializeComponent();
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoginDataListView.ItemsSource = await _handler.GetAllProductsAsync();
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MenuItem_Click_Add(object sender, RoutedEventArgs e)
        {
            var window = new AddProductWindow();
            window.ShowDialog();
        }

        private void OnMouseLeftReleased_Test(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item?.IsSelected == true)
            {
                var content = item.Content as Product;
                MessageBox.Show($"{content.Password}");
            }
        }
    }
}
