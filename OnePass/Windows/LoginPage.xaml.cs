﻿using OnePass.Infrastructure;
using OnePass.Services;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    [Inject]
    public partial class LoginPage : Page
    {
        private readonly ISettingsMonitor _settingsMonitor;

        public LoginPage(ISettingsMonitor settingsMonitor)
        {
            InitializeComponent();
            _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
        }

        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            var password = PasswordTextbox.Password;

            if (!IsValid(password))
            {
                MessageBox.Show("Invalid Password", "Invalid");
                return;
            }

            _settingsMonitor.Current.MasterPassword = password;

            // Redirect to view page
            var app = Application.Current as App;
            var window = Application.Current.MainWindow as MainWindow;
            window.Content = app.GetService<ViewPage>();
        }

        private bool IsValid(string password)
        {
            using var sha = SHA256.Create();
            var entered_bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + _settingsMonitor.Current.Salt));
            var entered_str = Encoding.UTF8.GetString(entered_bytes);

            if (_settingsMonitor.Current.Hash != entered_str)
            {
                return false;
            }

            return true;
        }
    }
}
