using OnePass.WPF.Models;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for ContentWindow.xaml
    /// </summary>
    public partial class ContentWindow : Window
    {
        public ContentWindow()
        {
            InitializeComponent();
            DataContext = new ContentModel();

            AccountsListView.Visibility = Visibility.Visible;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as ListView;
            var selected = item.SelectedItem as AccountListModel;

            //ProductDetailsGrid.Visibility = Visibility.Visible;
            //ProductDetailsGrid.DataContext = selected;
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click_AddAccount(object sender, RoutedEventArgs e)
        {
            var window = new AccountWindow(this, edit: false);
            window.Show();
        }

        private void MenuItem_Click_EditAccount(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = AccountsListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;
            var model = item.DataContext as AccountListModel;

            // Window
            var accountWindow = new AccountWindow(this, edit: true)
            {
                DataContext = new AccountModel()
                {
                    Guid = model.Guid,
                    Name = model.Name,
                    Username = model.Username,
                    EmailAddress = model.EmailAddress,
                    Password = model.Password,
                }
            };

            accountWindow.Show();
        }

        private void MenuItem_Click_RemoveAccount(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = AccountsListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;

            var confirm = MessageBox.Show("Delete account", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                if (item.DataContext is AccountListModel model)
                {
                    if (DataContext is ContentModel contentModel)
                    {
                        contentModel.Remove(model);
                    }
                }
            }
        }

        private void MenuItem_Click_ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow() { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void MenuItem_Click_CopyPassword(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = AccountsListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;
            var model = item.DataContext as AccountListModel;

            // Copy to clipboard
            if (!string.IsNullOrEmpty(model.Password))
            {
                Clipboard.SetText(model.Password);
            }
        }

        private void MenuItem_Click_ClearClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
        }
    }
}
