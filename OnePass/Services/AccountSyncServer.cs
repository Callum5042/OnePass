using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public class AccountSyncServer
    {
        public async Task ListenAsync()
        {
            var listener = new TcpListener(IPAddress.Any, port: 48750);
            listener.Start();

            var client = await listener.AcceptTcpClientAsync();
            using var network = client.GetStream();
            var reader = new BinaryReader(network);
            var writer = new BinaryWriter(network);

            // Read buffer size
            var bufferSize = reader.ReadInt32();

            // Send ok
            writer.Write(true);

            // Read buffer
            var buffer = reader.ReadBytes(bufferSize);
            var msg = Encoding.UTF8.GetString(buffer);

            // Send ok
            writer.Write(true);


            listener.Stop();
        }
    }
}
