using OnePass.Infrastructure;
using OnePass.Services;
using System.Windows;

namespace OnePass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceBuilder _serviceBuilder = new ServiceBuilder();

        public App()
        {
            var settings = GetService<ISettingsMonitor>();
            settings.Current.Test = "Test from App";
        }

        public T GetService<T>()
        {
            return _serviceBuilder.GetService<T>();
        }
    }
}
