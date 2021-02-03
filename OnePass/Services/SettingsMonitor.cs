using OnePass.Services.Interfaces;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public class SettingsMonitor : ISettingsMonitor
    {
        public OnePassSettings Current { get; set; }

        public SettingsMonitor()
        {
            // Current = new OnePassSettings();

            using var file = File.OpenRead(@"settings.json");
            using var reader = new StreamReader(file);
            var json = reader.ReadToEnd();

            Current = JsonSerializer.Deserialize<OnePassSettings>(json);
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(Current);

            using var file = File.Create(@"settings.json");
            using var writer = new StreamWriter(file);
            await writer.WriteAsync(json);
        }
    }
}
