using OnePass.Infrastructure;
using OnePass.Models;
using OnePass.Services;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.WPF.Handlers
{
    [Inject]
    public class SyncHandler
    {
        private readonly SyncServer _syncServer;
        private readonly AccountSyncer _accountSyncer;
        private readonly OnePassRepository _onePassRepository;

        public SyncHandler(SyncServer syncServer, AccountSyncer accountSyncer, OnePassRepository onePassRepository)
        {
            _syncServer = syncServer;
            _accountSyncer = accountSyncer;
            _onePassRepository = onePassRepository;
        }

        public async Task<SyncResults> HandleAsync()
        {
            await _syncServer.ListenAsync(port: 42655);

            // Awaits for client to send encrypted file
            var remoteAccounts = _syncServer.ReceivesAndDecryptData(_onePassRepository.MasterPassword);

            // Load local 
            var localAccounts = await GetLocalAccountsAsync(_onePassRepository.Filename, _onePassRepository.MasterPassword);

            // Sync accounts
            var syncedAccounts = _accountSyncer.Sync(localAccounts, remoteAccounts);

            // Save locally
            await SaveLocalAccountsAsync(_onePassRepository.Filename, _onePassRepository.MasterPassword, syncedAccounts);

            // Send to remote device
            _syncServer.EncryptAndSendData(_onePassRepository.MasterPassword, syncedAccounts);

            return new SyncResults()
            {
                Success = true
            };
        }

        private static async Task<IList<Account>> GetLocalAccountsAsync(string filename, string password)
        {
            using var input = File.OpenRead(filename);
            using var output = new MemoryStream();

            var encryptor = new FileEncryptor();
            await encryptor.DecryptAsync(input, output, password);

            output.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(output);
            var json = await reader.ReadToEndAsync();

            var accounts = JsonSerializer.Deserialize<List<Account>>(json);
            return accounts;
        }

        private static async Task SaveLocalAccountsAsync(string filename, string password, IEnumerable<Account> accounts)
        {
            var json = JsonSerializer.Serialize(accounts);

            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);

            using var file = File.OpenWrite(filename);
            file.SetLength(0);

            var encryptor = new FileEncryptor();
            await encryptor.EncryptAsync(memory, file, password);
        }
    }
}
