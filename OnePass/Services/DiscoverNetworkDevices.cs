using OnePass.Infrastructure;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace OnePass.Services
{
    [Inject(typeof(IDiscoverNetworkDevices))]
    public class DiscoverNetworkDevices : IDiscoverNetworkDevices
    {
        public async Task<IEnumerable<PingResult>> GetDevicesAsync()
        {
            var gateway = GetNetworkGateway(); // "192.168.0.1";

            // Enumerable IP Addresses
            var baseIP = gateway.Remove(gateway.Length - 1);
            var validAddresses = new ConcurrentBag<PingResult>();

            var taskList = new List<Task>();
            foreach (var number in Enumerable.Range(1, 255))
            {
                var task = Task.Run(async () =>
                {
                    var ipAddress = $"{baseIP}{number}";
                    var pingResult = await PingAddressAsync(ipAddress);
                    if (pingResult != null)
                    {
                        validAddresses.Add(pingResult);
                    }
                });

                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
            return validAddresses;
        }

        private static async Task<PingResult> PingAddressAsync(string ip)
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, 100);
            if (reply.Status == IPStatus.Success)
            {
                var hostEntry = await Dns.GetHostEntryAsync(ip);
                return new PingResult()
                {
                    IPAddress = reply.Address.ToString(),
                    HostName = hostEntry.HostName
                };
            }

            return null;
        }

        private static string GetNetworkGateway()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .Select(x => x.GetIPProperties())
                .SelectMany(x => x.GatewayAddresses)
                .Select(x => x.Address.ToString())
                .LastOrDefault();
        }
    }
}
