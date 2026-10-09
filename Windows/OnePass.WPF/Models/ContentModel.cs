using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ContentModel : ObservableObject
    {
        private readonly IFileEncoder _fileEncoder;
        private readonly UserData _onePassData;

        public IList<AccountListModel> AccountListModel { get; set; }

        public ContentModel(IFileEncoder fileEncoder, UserData onePassData)
        {
            _fileEncoder = fileEncoder;
            _onePassData = onePassData;
        }

        public async Task LoadAsync()
        {
            var root = _onePassData.InitialVaultData;
            _onePassData.InitialVaultData = null;

            if (root is null)
            {
                root = await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password, _onePassData.FilePath);
            }

            var accountListModel = root.Accounts
                .OrderByDescending(x => x.Favourite)
                .ThenBy(x => x.Name)
                .Select(x => new AccountListModel()
                {
                    Guid = x.Guid,
                    Name = x.Name,
                    Username = x.Username,
                    EmailAddress = x.EmailAddress,
                    Password = x.Password,
                    DateCreated = x.DateCreated,
                    DateModified = x.DateModified,
                    WebsiteUrl = x.WebsiteUrl,
                    Favourite = x.Favourite,
                    Notes = x.Notes,
                    PasswordHistory = x.PasswordHistory.Select(x => new PasswordHistoryModel() { Password = x.Password, DateSet = x.DateTime }).ToList()
                });

            AccountListModel = accountListModel.ToList();
            Accounts = new ObservableCollection<AccountListModel>(AccountListModel);
            CheckVisibility();
        }

        public ObservableCollection<AccountListModel> Accounts
        {
            get => accounts;
            set
            {
                // Replacing the list clears the list view's selection, so carry it across
                var previous = SelectedAccount;
                if (SetProperty(ref accounts, value))
                {
                    SelectedAccount = previous != null && value.Contains(previous) ? previous : value.FirstOrDefault();
                }
            }
        }
        private ObservableCollection<AccountListModel> accounts = new();

        public AccountListModel SelectedAccount { get => selectedAccount; set => SetProperty(ref selectedAccount, value); }
        private AccountListModel selectedAccount;

        // The inline add/edit form; null when just viewing
        public AccountModel Editor
        {
            get => editor;
            private set
            {
                if (SetProperty(ref editor, value))
                {
                    OnPropertyChanged(nameof(IsEditing));
                    OnPropertyChanged(nameof(IsNotEditing));
                    OnPropertyChanged(nameof(EditorTitle));
                }
            }
        }
        private AccountModel editor;

        // The account being edited; null when adding
        private AccountListModel editingAccount;

        public bool IsEditing => Editor != null;

        public bool IsNotEditing => Editor == null;

        public bool IsAdding => Editor != null && editingAccount == null;

        public string EditorTitle => editingAccount == null ? "New account" : "Edit account";

        public void BeginAdd()
        {
            editingAccount = null;
            Editor = new AccountModel(_fileEncoder, _onePassData);
        }

        public void BeginEdit(AccountListModel account)
        {
            var model = new AccountModel(_fileEncoder, _onePassData);
            model.Guid = account.Guid;
            model.Name = account.Name;
            model.Username = account.Username;
            model.EmailAddress = account.EmailAddress;
            model.Password = account.Password;
            model.Favourite = account.Favourite;
            model.Website = account.WebsiteUrl;
            model.Notes = account.Notes;

            editingAccount = account;
            SelectedAccount = account;
            Editor = model;
        }

        public void CancelEdit()
        {
            Editor = null;
            editingAccount = null;
        }

        public bool EditorHasChanges
        {
            get
            {
                if (Editor == null)
                {
                    return false;
                }

                var original = editingAccount;
                return !SameText(Editor.Name, original?.Name)
                    || !SameText(Editor.Username, original?.Username)
                    || !SameText(Editor.EmailAddress, original?.EmailAddress)
                    || !SameText(Editor.Password, original?.Password)
                    || !SameText(Editor.Website, original?.WebsiteUrl)
                    || !SameText(Editor.Notes, original?.Notes)
                    || Editor.Favourite != (original?.Favourite ?? false);
            }
        }

        private static bool SameText(string left, string right) => (string.IsNullOrEmpty(left) ? null : left) == (string.IsNullOrEmpty(right) ? null : right);

        /// <summary>Validates and saves the editor to the vault. Returns false if validation failed.</summary>
        public async Task<bool> SaveEditorAsync()
        {
            var model = Editor;
            if (model == null || !model.IsValid())
            {
                return false;
            }

            // Read the vault fresh so the save never overwrites changes made elsewhere since it was opened
            await model.LoadAsync();

            var now = DateTime.Now;
            var keepHistory = App.Current.AppOptions.EnablePasswordHistory;
            AccountListModel account;

            if (editingAccount == null)
            {
                var guid = await model.AddAccountAsync();
                account = new AccountListModel()
                {
                    Guid = guid,
                    DateCreated = now,
                };

                if (keepHistory)
                {
                    account.PasswordHistory.Add(new PasswordHistoryModel() { Password = model.Password, DateSet = now });
                }

                AccountListModel.Add(account);

                // Make sure the new account is visible
                search = null;
                OnPropertyChanged(nameof(Search));
            }
            else
            {
                await model.UpdateAccountAsync();
                account = editingAccount;

                if (account.Password != model.Password && keepHistory)
                {
                    account.AddPasswordHistory(new PasswordHistoryModel() { Password = model.Password, DateSet = now });
                }
            }

            account.Name = model.Name;
            account.Username = model.Username;
            account.EmailAddress = model.EmailAddress;
            account.Password = model.Password;
            account.Favourite = model.Favourite;
            account.WebsiteUrl = model.Website;
            account.Notes = model.Notes;
            account.DateModified = now;

            CancelEdit();

            // Re-sort in case the name or favourite changed
            AccountListModel = AccountListModel.OrderByDescending(x => x.Favourite).ThenBy(x => x.Name).ToList();
            ApplySearch();
            SelectedAccount = account;

            return true;
        }

        public async Task RemoveAsync(AccountListModel model)
        {
            var root = await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password, _onePassData.FilePath);

            root.DeletedAccounts.Add(model.Guid);

            var account = root.Accounts.First(x => x.Guid == model.Guid);
            root.Accounts.Remove(account);

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password, root, _onePassData.FilePath);

            // Remove from view
            Accounts.Remove(model);
            CheckVisibility();
        }

        private bool IsSearching => !string.IsNullOrWhiteSpace(search);

        public string EmptyStackPanelContent => IsSearching ? "No matching accounts" : "No accounts yet";

        public string EmptyStackPanelHint => IsSearching ? "Try a name, username or email." : "Use Add account to create your first one.";

        // Segoe Fluent Icons: Search / Contact
        public string EmptyStackPanelGlyph => IsSearching ? "" : "";

        public string AccountCountText
        {
            get
            {
                var total = AccountListModel?.Count ?? 0;
                return IsSearching ? $"{Accounts.Count} of {total}" : $"{total} saved";
            }
        }

        public string Search
        {
            get => search;
            set
            {
                SetProperty(ref search, value);
                ApplySearch();
            }
        }
        private string search;

        private void ApplySearch()
        {
            var value = search;
            if (string.IsNullOrWhiteSpace(value))
            {
                Accounts = new ObservableCollection<AccountListModel>(AccountListModel);
            }
            else
            {
                var filter = AccountListModel.Where(x =>
                {
                    if (x.Name?.Contains(value, StringComparison.CurrentCultureIgnoreCase) == true)
                    {
                        return true;
                    }

                    if (x.Username?.Contains(value, StringComparison.CurrentCultureIgnoreCase) == true)
                    {
                        return true;
                    }

                    if (x.EmailAddress?.Contains(value, StringComparison.CurrentCultureIgnoreCase) == true)
                    {
                        return true;
                    }

                    return false;
                });

                Accounts = new ObservableCollection<AccountListModel>(filter);
            }

            CheckVisibility();
        }

        public Visibility ListViewVisibility { get => listViewVisibility; set => SetProperty(ref listViewVisibility, value); }
        private Visibility listViewVisibility = Visibility.Collapsed;

        public Visibility EmptyStackPanelVisibility { get => emptyStackPanelVisibility; set => SetProperty(ref emptyStackPanelVisibility, value); }
        private Visibility emptyStackPanelVisibility = Visibility.Collapsed;

        public void CheckVisibility()
        {
            if (Accounts.Any())
            {
                ListViewVisibility = Visibility.Visible;
                EmptyStackPanelVisibility = Visibility.Collapsed;
            }
            else
            {
                ListViewVisibility = Visibility.Collapsed;
                EmptyStackPanelVisibility = Visibility.Visible;
            }

            if (SelectedAccount == null || !Accounts.Contains(SelectedAccount))
            {
                SelectedAccount = Accounts.FirstOrDefault();
            }

            OnPropertyChanged(nameof(AccountCountText));
            OnPropertyChanged(nameof(EmptyStackPanelContent));
            OnPropertyChanged(nameof(EmptyStackPanelHint));
            OnPropertyChanged(nameof(EmptyStackPanelGlyph));
        }
    }
}
