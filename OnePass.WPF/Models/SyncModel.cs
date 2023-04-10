using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OnePass.WPF.Models
{
    [Inject]
    public class SyncModel : ObservableValidator
    {
        private TcpListener Listener { get; set; }

        private CancellationTokenSource TcpCancellationTokenSource { get; set; }

        public SyncModel()
        {
            Listener = new TcpListener(IPAddress.Any, port: 13345);
        }

        public bool StartSyncButtonEnabled { get => startSyncButtonEnabled; set => SetProperty(ref startSyncButtonEnabled, value); }
        private bool startSyncButtonEnabled = true;

        public Visibility SpinnerVisibility { get => spinnerVisibility; set => SetProperty(ref spinnerVisibility, value); }
        private Visibility spinnerVisibility = Visibility.Collapsed;

        public string SyncStatus { get => syncStatus; set => SetProperty(ref syncStatus, value); }
        private string syncStatus;

        public string HostName { get => hostName; set => SetProperty(ref hostName, value); }
        private string hostName;

        public List<AccountListModel> AccountListModels { get; set; }

        public async Task<bool> SyncServer()
        {
            try
            {
                TcpCancellationTokenSource = new CancellationTokenSource();
                StartSyncButtonEnabled = false;
                SpinnerVisibility = Visibility.Visible;
                SyncDetailsVisibility = Visibility.Collapsed;

                SyncStatus = "Awaiting connection";

                HostName = Dns.GetHostName();

                Listener.Start();
                var client = await Listener.AcceptTcpClientAsync(TcpCancellationTokenSource.Token);
                SyncStatus = "Connected";

                using var stream = client.GetStream();
                using var reader = new BinaryReader(stream);

                // Read content
                var json = reader.ReadString();
                var mobileAccounts = JsonSerializer.Deserialize<OnePassData>(json);

                var accountModel = App.Current.GetService<AccountModel>();
                await accountModel.LoadAsync();

                var accountSyncer = new AccountSyncer();
                var sortedAccounts = accountSyncer.Sync(mobileAccounts, accountModel.OnePassData);

                // Send sorted content back
                var data = new OnePassData
                {
                    Accounts = sortedAccounts.Accounts,
                    DeletedAccounts = sortedAccounts.DeletedAccounts
                };

                using var writer = new BinaryWriter(stream);
                var sortedJson = JsonSerializer.Serialize(data);
                writer.Write(sortedJson);

                // Save
                var fileEncoder = App.Current.GetService<IFileEncoder>();
                var userData = App.Current.GetService<UserData>();
                await fileEncoder.SaveAsync(userData.Username, userData.Password, data);

                // Append
                AccountListModels = sortedAccounts.Accounts
                    .OrderByDescending(x => x.Favourite)
                    .ThenBy(x => x.Name)
                    .Select(x => new AccountListModel()
                    {
                        Guid = x.Guid,
                        Name = x.Name,
                        Username = x.Username,
                        EmailAddress = x.EmailAddress,
                        Password = x.Password,
                        DateModified = x.DateModified,
                        Favourite = x.Favourite,
                        PasswordHistory = x.PasswordHistory.Select(x => new PasswordHistoryModel() { Password = x.Password, DateSet = x.DateTime }).ToList()
                    })
                    .ToList();

                CloseListener();
                return true;
            }
            catch (Exception excetion)
            {
                SyncStatus = "Failed: " + excetion.Message;
            }
            finally
            {
                CloseListener();
            }

            return false;
        }

        public void CloseListener()
        {
            TcpCancellationTokenSource?.Cancel();
            Listener.Stop();
        }

        private Visibility syncDetailsVisibility = Visibility.Collapsed;

        public Visibility SyncDetailsVisibility { get => syncDetailsVisibility; set => SetProperty(ref syncDetailsVisibility, value); }

        private string syncDetails;

        public string SyncDetails { get => syncDetails; set => SetProperty(ref syncDetails, value); }
    }
}
