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

        public App()
        {
        }

        public T GetService<T>()
        {
            return _serviceBuilder.GetService<T>();
        }
    }
}
