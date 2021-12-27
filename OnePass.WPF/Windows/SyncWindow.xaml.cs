using OnePass.Services;
using OnePass.WPF.Handlers;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for SyncWindow.xaml
    /// </summary>
    public partial class SyncWindow : Window
    {
        //private TcpListener _listener;
        //private TcpClient _client;
        //private readonly SyncServer _syncServer;
        private readonly SyncHandler _syncHandler;

        public SyncWindow(SyncHandler syncHandler)
        {
            //_syncServer = syncServer;
            _syncHandler = syncHandler;

            InitializeComponent();
        }

        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            Textbox.Text = "Awaiting device to connect";
            var syncResults = await _syncHandler.HandleAsync();

            if (syncResults.Success)
            {
                Textbox.Text = "Devices have been synced";
            }
            else
            {
                throw new NotImplementedException();
            }

            // Load local encrypted file from filesystem



            // Get JSON from remote device
            //var remoteJson = await GetSyncedDeviceJSONAsync();
            //if (string.IsNullOrWhiteSpace(remoteJson))
            //{
            //    return;
            //}

            //var remoteAccounts = JsonSerializer.Deserialize<List<Account>>(remoteJson);

            //// Load local JSON
            //var localJson = await LoadLocalJSONAsync();
            //if (string.IsNullOrWhiteSpace(localJson))
            //{
            //    return;
            //}

            //var localAccounts = JsonSerializer.Deserialize<List<Account>>(localJson);

            //// Convert
            //var syncer = new AccountSyncer();
            //var jointAccounts = syncer.Sync(localAccounts, remoteAccounts);
            //var jointJson = JsonSerializer.Serialize(jointAccounts);
            //var buffer = Encoding.UTF8.GetBytes(jointJson);
            //var encryptor = GetEncryptor("password123");

            //// Encrypt and save locally
            //using var file = File.OpenWrite("Callum.bin");
            //file.SetLength(0);

            //using var localMemoryStream = new MemoryStream(buffer);
            //using var encryptStream = new CryptoStream(localMemoryStream, encryptor, CryptoStreamMode.Read);
            //await encryptStream.CopyToAsync(file);

            //// Encrypt and send to remote device
            //var networkStream = _client.GetStream();
            //var writer = new BinaryWriter(networkStream);
            //writer.Write("It has been done");

            // Success
            //Textbox.Text = "Devices have been synced";
        }

        //private static async Task<string> LoadLocalJSONAsync()
        //{
        //    using var input = File.OpenRead(@"Callum.bin");
        //    using var output = new MemoryStream();

        //    var encryptor = new FileEncryptor();
        //    await encryptor.DecryptAsync(input, output, "password123");

        //    output.Seek(0, SeekOrigin.Begin);
        //    var reader = new StreamReader(output);
        //    var json = await reader.ReadToEndAsync();

        //    return json;
        //}

        //private async Task<string> GetSyncedDeviceJSONAsync()
        //{
        //    try
        //    {
        //        // Start TCP listener
        //        _listener = new TcpListener(IPAddress.Any, 51111);
        //        _listener.Start();

        //        // Await for device to conntect
        //        Textbox.Text = "Awaiting device to connect";
        //        _client = await _listener.AcceptTcpClientAsync();
        //        _client.NoDelay = true;

        //        // Read message
        //        var network = _client.GetStream();
        //        var reader = new BinaryReader(network);
        //        var bufferCount = reader.ReadInt32();

        //        // Send ok
        //        var writer = new BinaryWriter(network);
        //        writer.Write(true);

        //        // Read encrypted stream
        //        var encryptedBuffer = reader.ReadBytes(bufferCount);

        //        // Decrypt in memory
        //        var decryptor = GetDecrypter("password123");
        //        var encryptedStream = new MemoryStream(encryptedBuffer);
        //        var cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);
        //        var cryptoReader = new StreamReader(cryptoStream);
        //        var json = cryptoReader.ReadToEnd();

        //        //var json = Encoding.UTF8.GetString(buffer);

        //        // Print message
        //        Textbox.Text = "Syncing";
        //        return json;
        //    }
        //    catch (SocketException)
        //    {
        //        // Swallow
        //        Textbox.Text = "Connection lost";
        //    }

        //    return null;
        //}

        private void Window_Closed(object sender, EventArgs e)
        {
            //_listener.Stop();
        }

        //private static ICryptoTransform GetEncryptor(string password)
        //{
        //    var (Key, IV) = GetKeyAndIv(password);

        //    using var aes = Aes.Create();
        //    aes.Key = Key;
        //    aes.IV = IV;
        //    aes.Padding = PaddingMode.PKCS7;

        //    return aes.CreateEncryptor(aes.Key, aes.IV);
        //}

        //private static ICryptoTransform GetDecrypter(string password)
        //{
        //    var (Key, IV) = GetKeyAndIv(password);

        //    using var aes = Aes.Create();
        //    aes.Key = Key;
        //    aes.IV = IV;
        //    aes.Padding = PaddingMode.PKCS7;

        //    return aes.CreateDecryptor(aes.Key, aes.IV);
        //}

        //private static (byte[] Key, byte[] IV) GetKeyAndIv(string password)
        //{
        //    var sha2 = new SHA256CryptoServiceProvider();

        //    var rawKey = Encoding.UTF8.GetBytes(password);
        //    var rawIV = Encoding.UTF8.GetBytes(password);

        //    var hashKey = sha2.ComputeHash(rawKey);
        //    var hashIV = sha2.ComputeHash(rawIV);

        //    Array.Resize(ref hashKey, 16);
        //    Array.Resize(ref hashIV, 16);
        //    return (hashKey, hashIV);
        //}
    }
}
