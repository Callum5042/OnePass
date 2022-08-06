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
        private readonly OnePassData _onePassData;

        public IList<AccountListModel> AccountListModel { get; set; }

        public ContentModel(IFileEncoder fileEncoder, OnePassData onePassData)
        {
            _fileEncoder = fileEncoder;
            _onePassData = onePassData;
        }

        public async Task LoadAsync()
        {
            var root = await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password);

            AccountListModel = root.Accounts.OrderBy(x => x.Name).Select(x => new AccountListModel()
            {
                Guid = x.Guid,
                Name = x.Name,
                Username = x.Username,
                EmailAddress = x.EmailAddress,
                Password = x.Password,
                DateModified = x.DateModified,
            }).ToList();

            Accounts = new ObservableCollection<AccountListModel>(AccountListModel);
            CheckVisibility();
        }

        public ObservableCollection<AccountListModel> Accounts { get => accounts; private set => SetProperty(ref accounts, value); }
        private ObservableCollection<AccountListModel> accounts = new();

        public async Task RemoveAsync(AccountListModel model)
        {
            var root = await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password);

            var account = root.Accounts.First(x => x.Guid == model.Guid);
            root.Accounts.Remove(account);

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password, root);

            // Remove from view
            Accounts.Remove(model);
            CheckVisibility();
        }

        public string EmptyStackPanelContent { get => emptyStackPanelContent; set => SetProperty(ref emptyStackPanelContent, value); }
        private string emptyStackPanelContent = "Accounts list is empty";

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
                    EmptyStackPanelContent = "Accounts list is empty";
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
                    EmptyStackPanelContent = "No search results found";
                }

                // Show what panel to show
                if (Accounts.Any())
                {
                    ListViewVisibility = Visibility.Visible;
                    EmptyStackPanelVisibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyStackPanelVisibility = Visibility.Visible;
                    ListViewVisibility = Visibility.Collapsed;
                }
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
        }
    }
}
