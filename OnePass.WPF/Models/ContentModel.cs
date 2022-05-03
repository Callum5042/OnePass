using Microsoft.Toolkit.Mvvm.Input;
using OnePass.Infrastructure;
using OnePass.WPF.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ContentModel
    {
        public ContentModel()
        {
            AddAccountCommand = new RelayCommand(AddAccount);
        }

        public ICommand AddAccountCommand { get; set; }

        public void AddAccount()
        {
            MessageBox.Show("Add Account");
        }

        public ObservableCollection<AccountListModel> ProductDetails { get; set; }
    }
}
