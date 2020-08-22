using System.Threading.Tasks;

namespace OnePass.Services
{
    public interface ISettingsMonitor
    {
        OnePassSettings Current { get; set; }

        Task SaveAsync();
    }
}