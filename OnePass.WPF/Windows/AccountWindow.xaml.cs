﻿using OnePass.WPF.Models;
using System;
using System.Linq;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for AddAccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private readonly ContentWindow _contentWindow;

        public AccountWindow(ContentWindow contentWindow, bool edit)
        {
            _contentWindow = contentWindow ?? throw new ArgumentNullException(nameof(contentWindow));

            InitializeComponent();
            Owner = _contentWindow;
            DataContext = new AccountModel();

            AddAccountButton.Visibility = edit ? Visibility.Collapsed : Visibility.Visible;
            EditAccountButton.Visibility = edit ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Button_Click_AddAccount(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountModel model)
            {
                model.AddAccount();

                // Update content window and return
                if (_contentWindow.DataContext is ContentModel contentModel)
                {
                    contentModel.Accounts.Add(new AccountListModel()
                    {
                        Name = model.Name,
                    });
                }

                Close();
            }
        }

        private void Button_Click_EditAccount(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountModel accountModel)
            {
                accountModel.RegisterAccount();

                // Update content window and return
                if (_contentWindow.DataContext is ContentModel contentModel)
                {
                    var accountListModel = contentModel.Accounts.First(x => x.Guid == accountModel.Guid);
                    accountListModel.Name = accountModel.Name;
                }

                Close();
            }
        }
    }
}
