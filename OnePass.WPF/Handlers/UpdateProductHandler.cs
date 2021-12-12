using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.WPF.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Account = OnePass.Models.Account;

namespace OnePass.Handlers
{
    [Inject(typeof(IUpdateProductHandler))]
    public class UpdateProductHandler : IUpdateProductHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;

        public UpdateProductHandler(IFileSystem fileSystem, IFileEncryptor encryptor, OnePassRepository onePassRepository)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
        }

        public async Task<IEnumerable<Account>> UpdateAsync(AccountViewModel model)
        {
            var accounts = await ReadJsonAsync();

            var account = accounts.FirstOrDefault(x => x.Id == model.Id);
            if (account is null)
            {
                throw new InvalidOperationException($"Account is null for {nameof(UpdateProductHandler)}");
            }

            account.Name = model.Name;
            account.Login = model.Login;
            account.Password = model.Password;
            account.DateModified = DateTime.Now;

            // Since we might have acounts created before this field, set the field to now if it was null
            if (account.DateCreated is null)
            {
                account.DateCreated = DateTime.Now;
            }

            await SaveJsonAsync(accounts);
            return accounts;
        }

        private async Task<IList<Account>> ReadJsonAsync()
        {
            using var input = _fileSystem.File.OpenRead(_onePassRepository.Filename);
            using var output = new MemoryStream();
            await _encryptor.DecryptAsync(input, output, _onePassRepository.MasterPassword);

            output.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(output);
            var json = await reader.ReadToEndAsync();

            var accounts = JsonSerializer.Deserialize<IList<Account>>(json);
            return accounts;
        }

        private async Task SaveJsonAsync(IList<Account> accounts)
        {
            var json = JsonSerializer.Serialize(accounts);

            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);

            using var file = _fileSystem.File.OpenWrite(_onePassRepository.Filename);
            file.SetLength(0);
            await _encryptor.EncryptAsync(memory, file, _onePassRepository.MasterPassword);
        }
    }
}
