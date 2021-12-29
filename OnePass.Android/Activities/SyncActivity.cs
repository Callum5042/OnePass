using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OnePass.Droid.Activities
{
    [Activity(Label = "OnePass", Theme = "@style/AppTheme.Header")]
    public class SyncActivity : Activity
    {
        private TextView SyncText { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_sync);

            // Cache sync textview
            SyncText = FindViewById<TextView>(Resource.Id.sync_found_device);

            // Register sync button
            var syncScanButton = FindViewById<Button>(Resource.Id.sync_scan_button);
            syncScanButton.Click += OnClick_Scan_DiscoverDevices;

            // TODO: Remove - only used for testing
            var buttonTemplate = FindViewById<Button>(Resource.Id.sync_button);
            var layout = FindViewById<LinearLayout>(Resource.Id.syncLinearLayout);
            
            var button = new Button(this);
            button.Text = $"Connect 192.168.0.7";

            button.Click += async (sender, e) => await ConnectToTCPAsync("192.168.0.7");

            layout.AddView(button);
        }

        private async void OnClick_Scan_DiscoverDevices(object sender, EventArgs e)
        {
            SyncText.Text = "Searching for device";

            // Check if wifi is on
            var profiles = Connectivity.ConnectionProfiles;
            if (!profiles.Contains(ConnectionProfile.WiFi))
            {
                SyncText.Text = "WiFi is not connected";
                return;
            }

            // Attempt to ping DHCP
            var pingGateway = new Ping();
            var gatewayResult = await pingGateway.SendPingAsync("192.168.0.1");
            if (gatewayResult.Status != IPStatus.Success)
            {
                SyncText.Text = "Could not connect to the gateway server '192.168.0.1'";
                return;
            }

            // Scan network for active devices
            var activeIPs = await DiscoverIPAddresses();
            SyncText.Text = "Devices found";

            // Create buttons to connect to
            var buttonTemplate = FindViewById<Button>(Resource.Id.sync_button);
            var layout = FindViewById<LinearLayout>(Resource.Id.syncLinearLayout);
            foreach (var ip in activeIPs)
            {
                var button = new Button(this);
                button.Text = $"Connect {ip.IPAddress}";
                //button.Background = buttonTemplate.Background;

                button.Click += async (sender, e) => await ConnectToTCPAsync(ip.IPAddress);

                layout.AddView(button);
            }
        }

        static async Task<IEnumerable<PingResult>> DiscoverIPAddresses()
        {
            // Get the default gateway
            var gateway = "192.168.0.1"; // NetworkGateway();

            // Enumerate IP Addresses
            var baseIP = gateway.Remove(gateway.Length - 1);
            var validAddresses = new ConcurrentBag<PingResult>();

            var taskList = new List<Task>();
            foreach (var number in Enumerable.Range(1, 255))
            {
                var task = Task.Run(async () =>
                {
                    var ip = $"{baseIP}{number}";

                    var ping = new Ping();
                    var reply = await ping.SendPingAsync(ip, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        try
                        {
                            // Try connect to port 
                            //using var client = new TcpClient();
                            //await client.ConnectAsync(ip, port: 42655);
                            //client.Close();

                            // Try get hostname
                            var hostname = string.Empty;
                            try
                            {
                                var hostEntry = Dns.GetHostEntry(ip);
                                hostname = hostEntry.HostName;
                            }
                            catch (SocketException ex)
                            {
                                hostname = ex.Message;
                            }

                            // Add
                            validAddresses.Add(new PingResult()
                            {
                                IPAddress = reply.Address.ToString(),
                                HostName = hostname
                            });
                        }
                        catch (SocketException)
                        {
                            // Swallow exception
                        }
                    }
                });

                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
            return validAddresses;
        }

        private class PingResult
        {
            public string IPAddress { get; set; }

            public string HostName { get; set; }
        }

        private async Task ConnectToTCPAsync(string ip)
        {
            try
            {
                Toast.MakeText(this, $"Connecting: {ip}", ToastLength.Short).Show();

                // Load file
                var buffer = await GetFileBufferAsync();

                // Connect
                var client = new TcpClient();
                await client.ConnectAsync(ip, 42655);

                using var network = client.GetStream();
                using var writer = new BinaryWriter(network);
                using var reader = new BinaryReader(network);

                // Write length of file
                writer.Write(buffer.Length);

                // Read ok
                reader.ReadBoolean();

                // Write buffer
                writer.Write(buffer);

                // Read ok
                reader.ReadBoolean();

                // Send ok
                writer.Write(true);

                // Accept new merged file
                var bufferSize = reader.ReadInt32();

                // Send ok
                writer.Write(true);

                // Get data
                var mergedBuffer = reader.ReadBytes(bufferSize);

                // Send ok
                writer.Write(true);

                // Save file
                await SaveAsync(mergedBuffer);

                // Success message
                //SyncText.Text = "Devices have been synced";
            }
            catch (SocketException ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
            }
        }

        private async Task<byte[]> GetFileBufferAsync()
        {
            var username = Intent.GetStringExtra("Username");
            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{username}.bin";
            var path = System.IO.Path.Combine(documentsPath, filename);

            using var file = File.OpenRead(path);
            using var memory = new MemoryStream();
            await file.CopyToAsync(memory);

            return memory.ToArray();
        }

        private Task SaveAsync(byte[] buffer)
        {
            //Toast.MakeText(this, "File saved", ToastLength.Short);

            var username = Intent.GetStringExtra("Username");
            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{username}.bin";
            var path = System.IO.Path.Combine(documentsPath, filename);

            //using var file = File.OpenWrite(path);
            using var file = new FileStream(path, FileMode.Open, FileAccess.Write);
            file.SetLength(0);

            using var writer = new BinaryWriter(file);
            writer.Write(buffer);
            writer.Flush();
            file.Flush();

            SyncText.Text = "Completed";
            return Task.CompletedTask;
        }
    }
}