using OnePass.Handlers;
using OnePass.Handlers.Interfaces;
using OnePass.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for ViewPage.xaml
    /// </summary>
    public partial class ViewPage : Page
    {
        private readonly IViewProductHandler _handler;
        private readonly IDeleteProductHandler _deleteProductHandler;

        public ObservableCollection<AccountViewModel> Products { get; set; } = new ObservableCollection<AccountViewModel>();

        public ViewPage(IViewProductHandler handler, IDeleteProductHandler deleteProductHandler)
        {
            InitializeComponent();
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _deleteProductHandler = deleteProductHandler ?? throw new ArgumentNullException(nameof(deleteProductHandler));

            LoginDataListView.ItemsSource = Products;
            Sort("Name", ListSortDirection.Ascending);
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
                    var content = item.Content as AccountViewModel;
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
                window.Product = item.Content as AccountViewModel;

                window.ShowDialog();
            }
        }

        private void MenuItem_Click_About(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var aboutWindow = app.GetService<AboutWindow>();
            aboutWindow.ShowDialog();
        }

        private void MenuItem_Click_ChangePassword(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            var window = app.GetService<ChangePasswordWindow>();
            window.ShowDialog();
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
                    if (item.Content is AccountViewModel product)
                    {
                        await _deleteProductHandler.DeleteProductAsync(product);
                        Products.Remove(product);
                    }
                }
            }
        }

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void LoginDataListView_Click(object sender, RoutedEventArgs e)
        {
            ListSortDirection direction;

            if (e.OriginalSource is GridViewColumnHeader headerClicked)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    // Choose direction  
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    // Sort
                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;
                    if (sortBy == "CensoredPassword")
                    {
                        // Don't want to sort by password
                        return;
                    }

                    Sort(sortBy, direction);

                    // Arrow?
                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    // Remember
                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            var dataView = CollectionViewSource.GetDefaultView(LoginDataListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void OnClick_Logout(object sender, RoutedEventArgs e)
        {
            //var app = Application.Current as App;

            //var window = app.GetService<LoginWindow>();
            //window.Show();

            //var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            //mainWindow.Close();
        }
    }
}
