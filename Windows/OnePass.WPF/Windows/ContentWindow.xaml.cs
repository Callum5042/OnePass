using OnePass.WPF.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
            var window = new AccountWindow(this, edit: false, historyTabSelected: false);
            window.Show();
        }

        private void MenuItem_Click_EditAccount(object sender, RoutedEventArgs e)
        {
            if (AccountFromMenu(sender) is AccountListModel model)
            {
                OpenEditAccountWindow(model, historyTab: false);
            }
        }

        private void Button_Click_EditSelectedAccount(object sender, RoutedEventArgs e)
        {
            if (SelectedAccount is AccountListModel model)
            {
                OpenEditAccountWindow(model, historyTab: false);
            }
        }

        private AccountListModel SelectedAccount => (DataContext as ContentModel)?.SelectedAccount;

        // A context menu inherits the data context of the row it was opened on
        private static AccountListModel AccountFromMenu(object sender) => (sender as MenuItem)?.DataContext as AccountListModel;

        private void OpenEditAccountWindow(AccountListModel model, bool historyTab)
        {
            var accountModel = App.Current.GetService<AccountModel>();
            accountModel.Guid = model.Guid;
            accountModel.Name = model.Name;
            accountModel.Username = model.Username;
            accountModel.EmailAddress = model.EmailAddress;
            accountModel.Password = model.Password;
            accountModel.Favourite = model.Favourite;
            accountModel.Website = model.WebsiteUrl;
            accountModel.Notes = model.Notes;
            accountModel.PasswordHistory = model.PasswordHistory.OrderByDescending(x => x.DateSet).ToList();

            var accountWindow = new AccountWindow(this, edit: true, historyTab)
            {
                DataContext = accountModel
            };

            accountWindow.Show();
        }

        private async void MenuItem_Click_RemoveAccount(object sender, RoutedEventArgs e)
        {
            if (AccountFromMenu(sender) is AccountListModel model)
            {
                await RemoveAccountAsync(model);
            }
        }

        private async void Button_Click_DeleteSelectedAccount(object sender, RoutedEventArgs e)
        {
            if (SelectedAccount is AccountListModel model)
            {
                await RemoveAccountAsync(model);
            }
        }

        private async Task RemoveAccountAsync(AccountListModel model)
        {
            var confirm = MessageBox.Show("Delete account", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                if (DataContext is ContentModel contentModel)
                {
                    contentModel.AccountListModel.Remove(model);
                    contentModel.Accounts.Remove(model);
                    await contentModel.RemoveAsync(model);
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
            CopyToClipboard(AccountFromMenu(sender)?.Username);
        }

        private void MenuItem_Click_CopyEmailAddress(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(AccountFromMenu(sender)?.EmailAddress);
        }

        private void MenuItem_Click_CopyPassword(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(AccountFromMenu(sender)?.Password);
        }

        private void Button_Click_CopySelectedUsername(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(SelectedAccount?.Username);
        }

        private void Button_Click_CopySelectedEmailAddress(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(SelectedAccount?.EmailAddress);
        }

        private void Button_Click_CopySelectedPassword(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(SelectedAccount?.Password);
        }

        private static void CopyToClipboard(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Clipboard.SetText(value);
            }
        }

        private void Hyperlink_Click_OpenSelectedWebsite(object sender, RoutedEventArgs e)
        {
            var uri = WebsiteUri(SelectedAccount?.WebsiteUrl);
            if (uri == null)
            {
                MessageBox.Show("Invalid URL", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Only open http(s) links; a bare domain like "github.com" is treated as https
        private static Uri WebsiteUri(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            url = url.Trim();
            if (!url.Contains("://"))
            {
                url = "https://" + url;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return uri;
            }

            return null;
        }

        private void AccountsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Never carry a revealed password over to another account
            RevealPasswordToggle.IsChecked = false;
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

        private void MenuItem_Click_ExportJson(object sender, RoutedEventArgs e)
        {
            var window = new VerifyWindow(this, new VerifyModel()
            {
                ButtonText = "Export JSON"
            });

            window.Show();
        }

        private void MenuItem_Click_Options(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigWindow()
            {
                Owner = this,
            };

            configWindow.ShowDialog();
        }

        private void MenuItem_Click_ChangePassword(object sender, RoutedEventArgs e)
        {
            if (AccountsListView.SelectedItem is AccountListModel accountModel)
            {
                var url = accountModel.WebsiteUrl;

                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = $"{url}/.well-known/change-password",
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid URL", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void MenuItem_Click_PasswordHistory(object sender, RoutedEventArgs e)
        {
            if (AccountsListView.SelectedItem is AccountListModel accountModel)
            {
                OpenEditAccountWindow(accountModel, historyTab: true);
            }
        }

        private void MenuItem_Click_SyncAccounts(object sender, RoutedEventArgs e)
        {
            var syncWindow = new SyncWindow(this)
            {
                Owner = this
            };

            syncWindow.ShowDialog();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusSearch();
            }
        }

        private void MenuItem_Click_FindAccount(object sender, RoutedEventArgs e)
        {
            FocusSearch();
        }

        private void Button_Click_ClearSearch(object sender, RoutedEventArgs e)
        {
            if (DataContext is ContentModel model)
            {
                model.Search = null;
            }

            SearchBox.Focus();
        }

        private void FocusSearch()
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
        }
    }
}
