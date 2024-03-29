﻿using OnePass.WPF.Models;
using System.Linq;
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
            DataContext = App.Current.GetService<ContentModel>();

            // AccountsListView.Visibility = Visibility.Visible;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var item = sender as ListView;
            //var selected = item.SelectedItem as AccountListModel;

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
            var accountModel = App.Current.GetService<AccountModel>();
            accountModel.Guid = model.Guid;
            accountModel.Name = model.Name;
            accountModel.Username = model.Username;
            accountModel.EmailAddress = model.EmailAddress;
            accountModel.Password = model.Password;

            var accountWindow = new AccountWindow(this, edit: true)
            {
                DataContext = accountModel
            };

            accountWindow.Show();
        }

        private async void MenuItem_Click_RemoveAccount(object sender, RoutedEventArgs e)
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
                        await contentModel.RemoveAsync(model);
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContentModel model)
            {
                await model.LoadAsync();
            }
        }
    }
}
