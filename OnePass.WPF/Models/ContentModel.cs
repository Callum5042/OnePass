using Microsoft.Toolkit.Mvvm.Input;
using OnePass.Infrastructure;
using OnePass.WPF.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ContentModel
    {
        public ContentModel()
        {
            //ProductDetails = new ObservableCollection<AccountListModel>()
            //{
            //    new AccountListModel() { Name = "Natwest" },
            //    new AccountListModel() { Name = "Microsoft" },
            //    new AccountListModel() { Name = "GMail" },
            //};
        }

        public ObservableCollection<AccountListModel> ProductDetails { get; set; }

        public Visibility EmptyAccountStackPanelVisibility { get; set; } = Visibility.Visible;

        public Visibility AccountListViewVisibility { get; set; } = Visibility.Collapsed;
    }
}
