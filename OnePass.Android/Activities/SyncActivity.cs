using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using OnePass.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OnePass.Droid.Activities
{
    [Activity(Label = "Sync Accounts", Theme = "@style/AppTheme.Header")]
    public class SyncActivity : Activity
    {
        private string Username { get; set; }

        private string Password { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_sync);

            Username = Intent.GetStringExtra(nameof(Username));
            Password = Intent.GetStringExtra(nameof(Password));

            var syncButton = FindViewById<Button>(Resource.Id.sync_button);
            syncButton.Click += SyncOnClick;
        }

        private async void SyncOnClick(object sender, EventArgs e)
        {
            try
            {
                await SyncAccounts();
            }
            catch (Exception ex)
            {
                var syncLayout = FindViewById<LinearLayout>(Resource.Id.sync_layout);
                syncLayout.Visibility = ViewStates.Visible;

                var syncButton = FindViewById<Button>(Resource.Id.sync_button);
                syncButton.Enabled = false;

                var syncStatus = FindViewById<TextView>(Resource.Id.sync_status);
                syncStatus.Text = ex.Message;
            }
        }

        private async Task SyncAccounts()
        {
            var syncLayout = FindViewById<LinearLayout>(Resource.Id.sync_layout);
            syncLayout.Visibility = ViewStates.Visible;

            var syncButton = FindViewById<Button>(Resource.Id.sync_button);
            syncButton.Enabled = false;

            var syncStatus = FindViewById<TextView>(Resource.Id.sync_status);
            syncStatus.Text = "Searching for connection";

            // Check if WiFi is connected
            if (!Connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi))
            {
                syncLayout.Visibility = ViewStates.Gone;
                syncButton.Enabled = true;
                syncStatus.Text = "WiFi is not turned on";
                return;
            }

            // Find IP
            var ip = FindIP(syncStatus);
            if (ip is null)
            {
                syncLayout.Visibility = ViewStates.Gone;
                syncButton.Enabled = true;
                return;
            }
            
            // Accept connection
            var client = new TcpClient();
            await client.ConnectAsync(ip.ToString(), port: 13345);
            syncStatus.Text = "Connected";

            using var stream = client.GetStream();
            using var writer = new BinaryWriter(stream);

            // Content
            var accounts = await GetAccountsAsync();
            var json = JsonConvert.SerializeObject(accounts);
            writer.Write(json);

            // Read sorted JSON
            using var reader = new BinaryReader(stream);
            var sortedJson = reader.ReadString();
            var data = JsonConvert.DeserializeObject<OnePass.Models.OnePassData>(sortedJson);

            // Update model
            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{Username}.bin";
            var path = Path.Combine(documentsPath, filename);

            var fileEncoder = new FileEncoder();
            await fileEncoder.SaveAsync(Username, Password, data, path);

            syncLayout.Visibility = ViewStates.Gone;
            syncButton.Enabled = true;
            syncStatus.Text = "Accounts has been synced";
        }

        private string FindIP(TextView syncStatus)
        {
            try
            {
                var hostname = FindViewById<EditText>(Resource.Id.hostname);
                if (string.IsNullOrWhiteSpace(hostname.Text))
                {
                    syncStatus.Text = "Hostname is empty";
                    return null;
                }

                var host = Dns.GetHostEntry(hostname.Text.Trim());
                var ip = host.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                return ip.ToString();
            }
            catch (Exception ex)
            {
                syncStatus.Text = ex.Message;
                return null;
            }
        }

        private async Task<OnePass.Models.OnePassData> GetAccountsAsync()
        {
            var fileEncoder = new FileEncoder();

            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{Username}.bin";
            var path = Path.Combine(documentsPath, filename);

            return await fileEncoder.LoadAsync(Username, Password, path);
        }
    }
}