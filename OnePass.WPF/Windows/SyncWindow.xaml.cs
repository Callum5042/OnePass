using OnePass.Models;
using OnePass.WPF.Infrastructure;
using OnePass.WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for SyncWindow.xaml
    /// </summary>
    public partial class SyncWindow : Window
    {
        private readonly ContentWindow _contentWindow;

        public SyncWindow(ContentWindow contentWindow)
        {
            InitializeComponent();
            DataContext = new SyncModel();
            _contentWindow = contentWindow;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var model = DataContext as SyncModel;
            model.CloseListener();
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as SyncModel;
            var result = await model.SyncServer();
            if (result)
            {
                model.SyncStatus = "Accounts has been synced";

                if (_contentWindow.DataContext is ContentModel contentModel)
                {
                    contentModel.AccountListModel = model.AccountListModels.ToList();
                    contentModel.Accounts = new ObservableCollection<AccountListModel>(model.AccountListModels);

                    contentModel.CheckVisibility();

                    model.StartSyncButtonEnabled = true;
                    model.SpinnerVisibility = Visibility.Collapsed;

                    model.SyncDetailsVisibility = Visibility.Visible;
                    model.SyncDetails = "Accounts has been synced";
                }
            }
        }
    }
}
