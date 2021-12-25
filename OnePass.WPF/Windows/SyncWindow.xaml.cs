using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            try
            {
                // Start TCP listener
                _listener = new TcpListener(IPAddress.Any, 51111);
                _listener.Start();

                // Await for device to conntect
                Textbox.Text = "Awaiting device to connect";
                _client = await _listener.AcceptTcpClientAsync();

                // Read stream
                using var networkStream = _client.GetStream();
                var reader = new StreamReader(networkStream);
                var msg = await reader.ReadLineAsync();

                // Print message
                Textbox.Text = msg;
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
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _listener.Stop();
        }
    }
}
