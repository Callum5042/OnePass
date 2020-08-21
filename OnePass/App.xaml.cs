using OnePass.Infrastructure;
using System.Windows;

namespace OnePass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceBuilder _serviceBuilder = new ServiceBuilder();

        public bool IsLoggedIn { get; set; } = false;

        public string MasterPassword { get; set; } = "password123";

        public string FileName { get; set; } = "data.bin";

        public App()
        {
        }

        public T GetService<T>()
        {
            return _serviceBuilder.GetService<T>();
        }
    }
}
