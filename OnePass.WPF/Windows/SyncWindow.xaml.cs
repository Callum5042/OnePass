using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for SyncWindow.xaml
    /// </summary>
    public partial class SyncWindow : Window
    {
        private TcpListener _listener;
        private TcpClient _client;

        public SyncWindow()
        {
            InitializeComponent();
        }

        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Get JSON from remote device
            var remoteJson = await GetSyncedDeviceJSONAsync();
            if (string.IsNullOrWhiteSpace(remoteJson))
            {
                return;
            }

            // Convert to files
            var remoteAccounts = JsonSerializer.Deserialize<List<Account>>(remoteJson);

            // Load local JSON
            var localJson = await LoadLocalJSONAsync();
            if (string.IsNullOrWhiteSpace(localJson))
            {
                return;
            }

            // Convert
            var localAccounts = JsonSerializer.Deserialize<List<Account>>(localJson);

            // Compare
            foreach (var account in localAccounts)
            {
                // Try find remote account
                var remote = remoteAccounts.FirstOrDefault(x => x.Id == account.Id);
                if (remote != null)
                {

                }
                else
                {
                    // Could not find 
                }
            }
        }

        private static async Task<string> LoadLocalJSONAsync()
        {
            using var input = File.OpenRead(@"Callum.bin");
            using var output = new MemoryStream();

            var encryptor = new FileEncryptor();
            await encryptor.DecryptAsync(input, output, "password123");

            output.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(output);
            var json = await reader.ReadToEndAsync();

            return json;
        }

        private async Task<string> GetSyncedDeviceJSONAsync()
        {
            try
            {
                // Start TCP listener
                _listener = new TcpListener(IPAddress.Any, 51111);
                _listener.Start();

                // Await for device to conntect
                Textbox.Text = "Awaiting device to connect";
                _client = await _listener.AcceptTcpClientAsync();

                // Decrypt file
                using var networkStream = _client.GetStream();
                using var decryptor = GetDecrypter("password123");
                using var cryptoStream = new CryptoStream(networkStream, decryptor, CryptoStreamMode.Read);
                var reader = new StreamReader(cryptoStream);
                var msg = await reader.ReadToEndAsync();

                // Print message
                Textbox.Text = "Syncing";
                return msg;
            }
            catch (SocketException)
            {
                // Swallow
                Textbox.Text = "Connection lost";
            }
            finally
            {
                if (_client?.Connected == true)
                {
                    _client.Close();
                }
            }

            return null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _listener.Stop();
        }

        private static ICryptoTransform GetDecrypter(string password)
        {
            var (Key, IV) = GetKeyAndIv(password);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Padding = PaddingMode.PKCS7;

            return aes.CreateDecryptor(aes.Key, aes.IV);
        }

        private static (byte[] Key, byte[] IV) GetKeyAndIv(string password)
        {
            var sha2 = new SHA256CryptoServiceProvider();

            var rawKey = Encoding.UTF8.GetBytes(password);
            var rawIV = Encoding.UTF8.GetBytes(password);

            var hashKey = sha2.ComputeHash(rawKey);
            var hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashKey, 16);
            Array.Resize(ref hashIV, 16);
            return (hashKey, hashIV);
        }
    }
}
