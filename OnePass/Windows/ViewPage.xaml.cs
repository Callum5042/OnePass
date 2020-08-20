using Microsoft.EntityFrameworkCore;
using OnePass.Services;
using OnePass.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ViewPage.xaml
    /// </summary>
    public partial class ViewPage : Page
    {
        public ViewPage()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // SeedData();

            //using var context = new OnePassContext();
            //var products = await context.Products.ToListAsync();
            //LoginDataListView.ItemsSource = products;

            LoginDataListView.ItemsSource = await ReadDatabaseAsync();
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MenuItem_Click_Add(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            var window = new AuthorWindow
            {
                Owner = mainWindow,
                ShowInTaskbar = false
            };

            window.ShowDialog();
        }

        private async Task<IEnumerable<Product>> ReadDatabaseAsync()
        {
            using var context = new OnePassContext();
            var products = await context.Products.ToListAsync();
            return products;
        }

        private async Task<IEnumerable<Product>> ReadJsonAsync()
        {
            // Read data
            using var file = File.OpenRead(@"data.json");
            using var reader = new StreamReader(file);
            var json = await reader.ReadToEndAsync();

            var root = JsonSerializer.Deserialize<ProductRoot>(json);
            return root.Products;
        }

        private void SeedData()
        {
            var product = new Product() { Name = "TestSite", Login = "Callum", Password = "password_123" };

            var root = new ProductRoot
            {
                Products = new List<Product>() { product }
            };

            var str = JsonSerializer.Serialize(root);

            using var file = File.OpenWrite("data.json");
            using var writer = new StreamWriter(file);
            writer.Write(str);
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
