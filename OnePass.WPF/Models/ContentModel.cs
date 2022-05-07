using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ContentModel : ObservableObject
    {
        private readonly FileEncoder _fileEncoder;
        private readonly OnePassData _onePassData;
        private IEnumerable<AccountListModel> _accountListModel;

        public ContentModel(FileEncoder fileEncoder, OnePassData onePassData)
        {
            _fileEncoder = fileEncoder;
            _onePassData = onePassData;
        }

        public async Task LoadAsync()
        {
            await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password);

            _accountListModel = _fileEncoder.Accounts.OrderBy(x => x.Name).Select(x => new AccountListModel()
            {
                Guid = x.Guid,
                Name = x.Name,
                Username = x.Username,
                EmailAddress = x.EmailAddress,
                Password = x.Password,
                DateModified = x.DateModified,
            });

            Accounts = new ObservableCollection<AccountListModel>(_accountListModel);
        }

        public ObservableCollection<AccountListModel> Accounts { get => accounts; private set => SetProperty(ref accounts, value); }
        private ObservableCollection<AccountListModel> accounts = new();

        public async Task RemoveAsync(AccountListModel model)
        {
            await _fileEncoder.LoadAsync(_onePassData.Username, _onePassData.Password);

            var account = _fileEncoder.Accounts.First(x => x.Guid == model.Guid);
            _fileEncoder.Accounts.Remove(account);

            await _fileEncoder.SaveAsync(_onePassData.Username, _onePassData.Password);

            // Remove from view
            Accounts.Remove(model);
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
                    Accounts = new ObservableCollection<AccountListModel>(_accountListModel);
                }
                else
                {
                    var filter = _accountListModel.Where(x =>
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
            }
        }
        private string search;
    }
}
