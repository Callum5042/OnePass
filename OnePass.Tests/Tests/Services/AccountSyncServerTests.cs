using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    //public class AccountSyncServerTests
    //{
    //    private class Client
    //    {
    //        private readonly TcpClient _client;

    //        public Client()
    //        {
    //            _client = new TcpClient();
    //        }

    //        public async Task ConnectAsync()
    //        {
    //            await _client.ConnectAsync("localhost", port: 48750);
    //        }

    //        public async Task SendAsync()
    //        {
    //            var network = _client.GetStream();
    //            var writer = new BinaryWriter(network);
    //            var reader = new BinaryReader(network);

    //            var buffer = Encoding.UTF8.GetBytes(Json());

    //            // Send buffer size to server
    //            writer.Write(buffer.Length);

    //            // Wait for ok
    //            if (!reader.ReadBoolean())
    //            {
    //                throw new InvalidOperationException("Big boom");
    //            }

    //            // Send buffer to server
    //            writer.Write(buffer);

    //            // Wait for ok
    //            if (!reader.ReadBoolean())
    //            {
    //                throw new InvalidOperationException("Big boom");
    //            }
    //        }

    //        private static string Json()
    //        {
    //            var accounts = new List<Account>
    //            {
    //                new Account()
    //                {
    //                    Id = 1,
    //                    Name = "Product",
    //                    Login = "Login123",
    //                    Password = "Password123",
    //                    DateCreated = new DateTime(2010, 3, 10),
    //                    DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
    //                }
    //            };

    //            var json = JsonSerializer.Serialize(accounts);
    //            return json;
    //        }
    //    }

    //    [Fact]
    //    public async Task ListenAsync()
    //    {
    //        // Setup server
    //        _ = Task.Run(async () =>
    //        {
    //            var server = new AccountSyncServer();
    //            await server.ListenAsync();
    //        });

    //        // Start client
    //        var client = new Client();
    //        await client.ConnectAsync();
    //        await client.SendAsync();
    //    }
    //}
}
