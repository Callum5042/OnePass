using OnePass.Models;
using OnePass.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for ContentWindow.xaml
    /// </summary>
    public partial class ContentWindow : Window
    {
        public ObservableCollection<AccountListModel> Accounts { get; set; } = new ObservableCollection<AccountListModel>();

        public ContentWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as ListView;
            var selected = item.SelectedItem as AccountListModel;

            ProductDetailsGrid.Visibility = Visibility.Visible;
            ProductDetailsGrid.DataContext = selected;
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnLoadedWindow(object sender, RoutedEventArgs e)
        {
            // Load accounts
            var root = ReadFile();
            Accounts = new ObservableCollection<AccountListModel>(root.Accounts.Select(x => new AccountListModel()
            {
                Guid = x.Guid,
                Name = x.Name,
                Username = x.Username,
                EmailAddress = x.EmailAddress,
            }));

            AccountsListView.ItemsSource = Accounts;

            // Show empty label
            if (!Accounts.Any())
            {
                AccountsListView.Visibility = Visibility.Collapsed;
                AccountsEmptyStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AccountsListView.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_AddAccount(object sender, RoutedEventArgs e)
        {
            var window = new AccountWindow
            {
                Owner = this
            };

            window.Show();
        }

        private static RootAccount ReadFile()
        {
            var fileSignature = ".ONEPASS";
            var filename = App.Current.Filename;

            using var file = File.OpenRead(filename);
            using var reader = new BinaryReader(file);

            // Read signature
            var signature = reader.ReadBytes(Encoding.UTF8.GetByteCount(fileSignature));
            if (Encoding.UTF8.GetString(signature) != fileSignature)
            {
                throw new InvalidOperationException("Not a valid OnePass file");
            }

            // Read version
            var version = reader.ReadInt32();

            // Read password hash
            var passwordHashLength = reader.ReadInt32();
            var passwordHash = reader.ReadBytes(passwordHashLength);

            // Read salt
            var saltLength = reader.ReadInt32();
            var salt = reader.ReadBytes(saltLength);

            // Read IV
            var ivLength = reader.ReadInt32();
            var iv = reader.ReadBytes(ivLength);

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(App.Current.Password, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);
            aes.IV = iv;

            // Decrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var cryptoReader = new StreamReader(cryptoStream);
            var content = cryptoReader.ReadToEnd();

            var root = JsonSerializer.Deserialize<RootAccount>(content);
            return root;
        }

        private void MenuItem_Click_EditAccount(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            var item = AccountsListView.ItemContainerGenerator.ContainerFromItem(menu.DataContext) as ListViewItem;

            var model = item.DataContext as AccountListModel;

            // Window
            var accountWindow = new AccountWindow() { Owner = this };
            accountWindow.DataContext = model;

            accountWindow.Show();
        }
    }
}
