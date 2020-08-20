using OnePass.Handlers;
using OnePass.Infrastructure;
using OnePass.Services;
using System;
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

        public ViewPage(IViewProductHandler handler)
        {
            InitializeComponent();
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public async Task UpdateProductListAsync()
        {
            LoginDataListView.ItemsSource = await _handler.GetAllProductsAsync();
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

                    var popupText = new TextBlock
                    {
                        Text = "Password copied to clipboard",
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

                    var panel = new StackPanel();
                    panel.Background = Brushes.GhostWhite;

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
            }
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

        private void OnClick_DeleteProduct(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = LoginDataListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;

            if (item?.IsSelected == true)
            {
                var result = MessageBox.Show("Are you sure you want to delete this product?\nOnce deleted it cannot be recovered", "Delete Product", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
