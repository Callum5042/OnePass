using OnePass.Handlers.Interfaces;
using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.WPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Account = OnePass.Models.Account;

namespace OnePass.Handlers
{
    [Inject(typeof(IAddProductHandler))]
    public class AddProductHandler : IAddProductHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileEncryptor _encryptor;
        private readonly OnePassRepository _onePassRepository;

        public AddProductHandler(IFileSystem fileSystem, IFileEncryptor encryptor, OnePassRepository onePassRepository)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            _onePassRepository = onePassRepository ?? throw new ArgumentNullException(nameof(onePassRepository));
        }

        public async Task<IEnumerable<Account>> AddProductAsync(AccountViewModel model)
        {
            var accounts = await ReadJsonAsync();
            accounts.Add(new Account()
            {
                Name = model.Name,
                Login = model.Login,
                Password = model.Password,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            });

            for (int i = 0; i < accounts.Count; i++)
            {
                accounts[i].Id = i + 1;
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
