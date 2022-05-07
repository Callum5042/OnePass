using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ContentModel : ObservableObject
    {
        private readonly FileEncoder _fileEncoder;
        private readonly IEnumerable<AccountListModel> _accountListModel;

        public ContentModel()
        {
            _fileEncoder = new FileEncoder();
            _fileEncoder.Load();

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
        private ObservableCollection<AccountListModel> accounts = new ObservableCollection<AccountListModel>();

        public void Remove(AccountListModel model)
        {
            _fileEncoder.Load();

            var account = _fileEncoder.Accounts.First(x => x.Guid == model.Guid);
            _fileEncoder.Accounts.Remove(account);
            _fileEncoder.Save();

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
