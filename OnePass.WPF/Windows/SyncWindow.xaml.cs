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
        private readonly SyncHandler _syncHandler;

        public SyncWindow(SyncHandler syncHandler)
        {
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
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }
    }
}
