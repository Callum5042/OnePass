using System.Threading.Tasks;

namespace OnePass.Services.Interfaces
{
    public interface ISettingsMonitor
    {
        OnePassSettings Current { get; set; }

        Task SaveAsync();
    }
}