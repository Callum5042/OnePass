using OnePass.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public class SyncServer : IDisposable
    {
        private TcpListener _listener;
        private TcpClient _client;

        public void Dispose()
        {

        }

        public async Task ListenAsync(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            _client = await _listener.AcceptTcpClientAsync();
        }

        public IList<Account> ReceivesAndDecryptData(string password)
        {
            var memory = _client.GetStream();
            var writer = new BinaryWriter(memory);
            var reader = new BinaryReader(memory);

            // Get buffer size
            var bufferSize = reader.ReadInt32();

            // Sends OK to client
            writer.Write(true);

            // Get buffer
            var encryptedBuffer = reader.ReadBytes(bufferSize);

            // Sends OK to client
            writer.Write(true);

            // Decrypt in memory
            var decryptor = GetDecrypter(password);
            var encryptedStream = new MemoryStream(encryptedBuffer);
            var cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);
            var cryptoReader = new StreamReader(cryptoStream);
            var json = cryptoReader.ReadToEnd();

            var accounts = JsonSerializer.Deserialize<List<Account>>(json);
            return accounts;
        }

        public void EncryptAndSendData(string password, IEnumerable<Account> accounts)
        {
            var memory = _client.GetStream();
            var writer = new BinaryWriter(memory);
            var reader = new BinaryReader(memory);

            // Serialize accounts
            var json = JsonSerializer.Serialize(accounts);
            using var encryptor = GetEncrypter(password);
            using var cryptoMemory = new MemoryStream();
            using var crypto = new CryptoStream(cryptoMemory, encryptor, CryptoStreamMode.Write);
            using var cryptoWriter = new StreamWriter(crypto);
            cryptoWriter.Write(json);
            cryptoWriter.Flush();
            crypto.FlushFinalBlock();

            var buffer = cryptoMemory.ToArray();

            // Send buffer size
            writer.Write(buffer.Length);

            // Await ok
            reader.ReadBoolean();

            // Send buffer
            writer.Write(buffer);


            // Await ok
            reader.ReadBoolean();
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

        private static ICryptoTransform GetEncrypter(string password)
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
}
