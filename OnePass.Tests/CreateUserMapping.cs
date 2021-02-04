//using OnePass.Models;
//using System;
//using System.IO;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace OnePass.Tests
//{
//    public class CreateUserMapping : IDisposable
//    {
//        private readonly string _filename;

//        public CreateUserMapping(string filename)
//        {
//            _filename = filename;
//        }

//        public async Task WriteAsync(AccountRoot accountRoot)
//        {
//            using var file = File.OpenWrite(_filename);
//            using var writer = new StreamWriter(file);

//            var json = JsonSerializer.Serialize(accountRoot);
//            await writer.WriteAsync(json);
//        }

//        public void Dispose()
//        {
//            if (File.Exists(_filename))
//            {
//                File.Delete(_filename);
//            }

//            GC.SuppressFinalize(this);
//        }
//    }
//}
