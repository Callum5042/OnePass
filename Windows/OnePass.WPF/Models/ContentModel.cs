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

        public ObservableCollection<AccountListModel> Accounts { get => accounts; set => SetProperty(ref accounts, value); }
        private ObservableCollection<AccountListModel> accounts = new();

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

                // Filter list
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
        }
        private string search;

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

            OnPropertyChanged(nameof(AccountCountText));
            OnPropertyChanged(nameof(EmptyStackPanelContent));
            OnPropertyChanged(nameof(EmptyStackPanelHint));
            OnPropertyChanged(nameof(EmptyStackPanelGlyph));
        }
    }
}
