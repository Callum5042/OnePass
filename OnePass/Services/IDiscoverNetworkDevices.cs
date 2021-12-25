using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public interface IDiscoverNetworkDevices
    {
        Task<IEnumerable<PingResult>> GetDevicesAsync();
    }
}