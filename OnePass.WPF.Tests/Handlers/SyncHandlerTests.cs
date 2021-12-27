using OnePass.Models;
using OnePass.Services;
using OnePass.WPF.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.WPF.Tests.Handlers
{
    public class SyncHandlerTests
    {
        private class TcpTestClient
        {
            public void Connect()
            {
                using var client = new TcpClient();
                client.Connect("localhost", 42655);

                var network = client.GetStream();
                var writer = new BinaryWriter(network);
                var reader = new BinaryReader(network);

                // Encrypt and send to server
                var json = GetJson();
                var encryptor = GetEncryptor("password123");

                using var cryptoMemory = new MemoryStream();
                using var cryptoStream = new CryptoStream(cryptoMemory, encryptor, CryptoStreamMode.Write);
                using var cryptoWriter = new StreamWriter(cryptoStream);
                cryptoWriter.Write(json);
                cryptoWriter.Flush();
                cryptoStream.FlushFinalBlock();

                var buffer = cryptoMemory.ToArray();

                // Send buffer length to server
                writer.Write(buffer.Length);

                // Read ok
                reader.ReadBoolean();

                // Send buffer
                writer.Write(buffer);

                // Read ok
                reader.ReadBoolean();
            }

            private static string GetJson()
            {
                var accounts = new List<Account>
                {
                    new Account()
                    {
                        Id = 1,
                        Name = "Product",
                        Login = "Login123",
                        Password = "Password123",
                        DateCreated = new DateTime(2010, 3, 10),
                        DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
                    }
                };

                var json = JsonSerializer.Serialize(accounts);
                return json;
            }

            private static ICryptoTransform GetEncryptor(string password)
            {
                var (Key, IV) = GetKeyAndIv(password);

                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;
                aes.Padding = PaddingMode.PKCS7;

                return aes.CreateEncryptor(aes.Key, aes.IV);
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

        [Fact]
        public async Task HandleAsync()
        {
            // Act
            //var task = Task.Run(async () =>
            //{
            //    var syncServer = new SyncServer();

            //    var handler = new SyncHandler(syncServer);
            //    var result = await handler.HandleAsync();

            //    // Assert
            //    Assert.NotNull(result);
            //});

            //var client = new TcpTestClient();
            //client.Connect();

            //await task;
        }
    }
}
