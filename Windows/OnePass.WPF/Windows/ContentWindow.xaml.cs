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
using System.Windows.Media.Animation;
using System.Windows.Threading;

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

        private ContentModel Model => DataContext as ContentModel;

        private void MenuItem_Click_AddAccount(object sender, RoutedEventArgs e)
        {
            if (Model is ContentModel model && !model.IsEditing)
            {
                model.BeginAdd();
                FocusEditor();
            }
        }

        private void MenuItem_Click_EditAccount(object sender, RoutedEventArgs e)
        {
            if (AccountFromMenu(sender) is AccountListModel account)
            {
                BeginEdit(account);
            }
        }

        private void Button_Click_EditSelectedAccount(object sender, RoutedEventArgs e)
        {
            if (SelectedAccount is AccountListModel account)
            {
                BeginEdit(account);
            }
        }

        private AccountListModel SelectedAccount => Model?.SelectedAccount;

        // A context menu inherits the data context of the row it was opened on
        private static AccountListModel AccountFromMenu(object sender) => (sender as MenuItem)?.DataContext as AccountListModel;

        private void BeginEdit(AccountListModel account)
        {
            if (Model is ContentModel model && !model.IsEditing)
            {
                model.BeginEdit(account);
                FocusEditor();
            }
        }

        private void FocusEditor()
        {
            EditorRevealPasswordToggle.IsChecked = false;

            // The editor only becomes visible on the next layout pass
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                EditorNameTextBox.Focus();
                EditorNameTextBox.CaretIndex = EditorNameTextBox.Text.Length;
            });
        }

        private async void Button_Click_SaveAccount(object sender, RoutedEventArgs e)
        {
            await SaveEditorAsync();
        }

        private async Task SaveEditorAsync()
        {
            if (Model is not ContentModel model || !model.IsEditing || !SaveAccountButton.IsEnabled)
            {
                return;
            }

            var adding = model.IsAdding;
            SaveAccountButton.IsEnabled = false;

            try
            {
                if (await model.SaveEditorAsync())
                {
                    ShowToast(adding ? "Account added" : "Account saved");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Couldn't save the account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveAccountButton.IsEnabled = true;
            }
        }

        private void Button_Click_CancelEdit(object sender, RoutedEventArgs e)
        {
            Model?.CancelEdit();
        }

        /// <summary>Returns true if there is nothing unsaved, or the user agreed to throw it away.</summary>
        private bool ConfirmDiscardEdits()
        {
            if (Model?.EditorHasChanges != true)
            {
                return true;
            }

            var result = MessageBox.Show(this, "Discard your unsaved changes?", "Unsaved changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        private void Button_Click_GeneratePassword(object sender, RoutedEventArgs e)
        {
            try
            {
                Model?.Editor?.GeneratePassword();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditorPasswordTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Don't let a hidden password be copied out of the field
            if (EditorRevealPasswordToggle.IsChecked != true && (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut))
            {
                e.Handled = true;
            }
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
            var confirm = MessageBox.Show(this, $"Delete {model.DisplayName}? This can't be undone.", "Delete account", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
            CopyToClipboard(AccountFromMenu(sender)?.Username, "Username");
        }

        private void MenuItem_Click_CopyEmailAddress(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(AccountFromMenu(sender)?.EmailAddress, "Email address");
        }

        private void MenuItem_Click_CopyPassword(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(AccountFromMenu(sender)?.Password, "Password");
        }

        private void Button_Click_CopySelectedUsername(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(SelectedAccount?.Username, "Username");
        }

        private void Button_Click_CopySelectedEmailAddress(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(SelectedAccount?.EmailAddress, "Email address");
        }

        private void Button_Click_CopySelectedPassword(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(SelectedAccount?.Password, "Password");
        }

        private void CopyToClipboard(string value, string what)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Clipboard.SetText(value);
                ShowToast($"{what} copied to clipboard");
            }
        }

        private void ShowToast(string message)
        {
            ToastText.Text = message;

            // Fade in, hold, fade out; starting again restarts the timing
            var fade = new DoubleAnimationUsingKeyFrames();
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2200))));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2500))));
            Toast.BeginAnimation(OpacityProperty, fade);
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
            if (!ConfirmDiscardEdits())
            {
                e.Cancel = true;
                return;
            }

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
            if (AccountFromMenu(sender) is AccountListModel model && DataContext is ContentModel contentModel)
            {
                contentModel.SelectedAccount = model;
                HistoryTab.IsSelected = true;
            }
        }

        private void Button_Click_CopyHistoryPassword(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(((sender as FrameworkElement)?.DataContext as PasswordHistoryModel)?.Password, "Password");
        }

        private void MenuItem_Click_SyncAccounts(object sender, RoutedEventArgs e)
        {
            var syncWindow = new SyncWindow(this)
            {
                Owner = this
            };

            syncWindow.ShowDialog();
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Model?.IsEditing == true)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
                {
                    e.Handled = true;
                    await SaveEditorAsync();
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    if (ConfirmDiscardEdits())
                    {
                        Model.CancelEdit();
                    }
                }

                return;
            }

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
