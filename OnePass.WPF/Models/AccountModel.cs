using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.WPF.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OnePass.WPF.Models
{
    [Inject]
    public class AccountModel : ObservableValidator
    {
        private readonly FileEncoder _fileEncoder;

        public AccountModel()
        {
            _fileEncoder = new FileEncoder();
            _fileEncoder.Load();
        }

        public Guid Guid { get; set; }

        [Required]
        public string Name { get => name; set => SetProperty(ref name, value); }
        private string name;

        public string Username { get => username; set => SetProperty(ref username, value); }
        private string username;

        public Guid AddAccount()
        {
            var guid = Guid.NewGuid();
            _fileEncoder.Accounts.Add(new Account()
            {
                Guid = guid,
                Name = Name,
            });

            _fileEncoder.Save();
            return guid;
        }

        public void RegisterAccount()
        {
            var account = _fileEncoder.Accounts.First(x => x.Guid == Guid);
            account.Name = Name;

            _fileEncoder.Save();
        }
    }
}
