using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ContentModel : ObservableObject
    {
        private readonly FileEncoder _fileEncoder;

        public ContentModel()
        {
            _fileEncoder = new FileEncoder();
            _fileEncoder.Load();

            Accounts = new ObservableCollection<AccountListModel>(_fileEncoder.Accounts.Select(x => new AccountListModel()
            {
                Guid = x.Guid,
                Name = x.Name,
                Username = x.Username,
                EmailAddress = x.EmailAddress,
                DateModified = x.DateModified,
            }));
        }

        public ObservableCollection<AccountListModel> Accounts { get; private set; } = new ObservableCollection<AccountListModel>();

        public void Remove(AccountListModel model)
        {
            _fileEncoder.Load();

            var account = _fileEncoder.Accounts.First(x => x.Guid == model.Guid);
            _fileEncoder.Accounts.Remove(account);
            _fileEncoder.Save();

            // Remove from view
            Accounts.Remove(model);
        }
    }
}
