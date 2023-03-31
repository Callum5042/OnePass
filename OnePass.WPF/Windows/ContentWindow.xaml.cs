using OnePass.WPF.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            OpenEditAccountWindow(model);
        }

        private void OpenEditAccountWindow(AccountListModel model)
        {
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
                        contentModel.AccountListModel.Remove(model);
                        contentModel.Accounts.Remove(model);
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

        private void MenuItem_Click_CopyUsername(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = AccountsListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;
            var model = item.DataContext as AccountListModel;

            // Copy to clipboard
            if (!string.IsNullOrEmpty(model.Username))
            {
                Clipboard.SetText(model.Username);
            }
        }

        private void MenuItem_Click_CopyEmailAddress(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = AccountsListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;
            var model = item.DataContext as AccountListModel;

            // Copy to clipboard
            if (!string.IsNullOrEmpty(model.EmailAddress))
            {
                Clipboard.SetText(model.EmailAddress);
            }
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

            // Resize window
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            AppOptions options = null;
            using var file = File.OpenRead(path);
            options = JsonSerializer.Deserialize<AppOptions>(file);
            
            if (options?.WindowMaximized == true)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                if (options?.WindowWidth != null && options?.WindowHeight != null)
                {
                    Width = (double)options.WindowWidth;
                    Height = (double)options.WindowHeight;
                    Left = (double)options.WindowPositionX;
                    Top = (double)options.WindowPositionY;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is ContentModel model)
                {
                    model.Search = null;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Read file
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            AppOptions options = null;
            using (var file = File.OpenRead(path))
            {
                options = JsonSerializer.Deserialize<AppOptions>(file);
            }

            // Save file
            options.WindowWidth = (int)Width;
            options.WindowHeight = (int)Height;
            options.WindowPositionX = (int)Left;
            options.WindowPositionY = (int)Top;
            options.WindowMaximized = WindowState == WindowState.Maximized;

            using (var file = File.Open(path, FileMode.Truncate))
            {
                JsonSerializer.Serialize(file, options);
            }
        }

        private void Toolbar_Click_EditButton(object sender, RoutedEventArgs e)
        {
            if (AccountsListView.SelectedItem is AccountListModel accountModel)
            {
                OpenEditAccountWindow(accountModel);
            }
        }

        private void MenuItem_Click_ExportJson(object sender, RoutedEventArgs e)
        {
            var window = new VerifyWindow(this, new VerifyModel()
            {
                ButtonText = "Export JSON"
            });

            window.Show();
        }
    }
}
