using OnePass.Handlers;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ViewPage.xaml
    /// </summary>
    [Inject]
    public partial class ViewPage : Page
    {
        private readonly IViewProductHandler _handler;
        private readonly IDeleteProductHandler _deleteProductHandler;

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        public ViewPage(IViewProductHandler handler, IDeleteProductHandler deleteProductHandler)
        {
            InitializeComponent();
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _deleteProductHandler = deleteProductHandler ?? throw new ArgumentNullException(nameof(deleteProductHandler));

            LoginDataListView.ItemsSource = Products;
        }

        public async Task UpdateProductListAsync()
        {
            Products.Clear();

            var list = await _handler.GetAllProductsAsync();
            foreach (var item in list)
            {
                Products.Add(item);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await UpdateProductListAsync();
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MenuItem_Click_Add(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;

            var window = app.GetService<AddProductWindow>();
            window.ShowDialog();
        }

        private async void OnMouseLeftDown_CopyPassword(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var item = sender as ListViewItem;
                if (item?.IsSelected == true)
                {
                    var content = item.Content as Product;
                    Clipboard.SetText(content.Password);
                    await CreatePopup("Password copied to clipboard");
                }
            }
        }

        private static async Task CreatePopup(string text)
        {
            var popupText = new TextBlock
            {
                Text = text,
                Background = Brushes.GhostWhite,
                Foreground = Brushes.Black,
                Margin = new Thickness(3),
            };

            var border = new Border()
            {
                Child = popupText,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray,
                CornerRadius = new CornerRadius(3)
            };

            var panel = new StackPanel
            {
                Background = Brushes.GhostWhite
            };

            panel.Children.Add(border);

            var codePopup = new Popup
            {
                Child = panel,
                Placement = PlacementMode.Mouse,
                AllowsTransparency = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsOpen = true,
            };

            await Task.Delay(1000);
            codePopup.IsOpen = false;
        }

        private async void MenuItem_OnClick_Refresh(object sender, RoutedEventArgs e)
        {
            await UpdateProductListAsync();
        }

        private void OnClick_UpdateProduct(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = LoginDataListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;

            if (item?.IsSelected == true)
            {
                var app = Application.Current as App;
                var window = app.GetService<UpdateProductWindow>();
                window.Product = item.Content as Product;

                window.ShowDialog();
            }
        }

        private async void OnClick_DeleteProduct(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = LoginDataListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;

            if (item?.IsSelected == true)
            {
                var result = MessageBox.Show("Are you sure you want to delete this product?\nOnce deleted it cannot be recovered", "Delete Product", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (item.Content is Product product)
                    {
                        await _deleteProductHandler.DeleteProductAsync(product);
                        Products.Remove(product);
                    }
                }
            }
        }
    }
}
