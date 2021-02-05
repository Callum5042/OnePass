using OnePass.Infrastructure;
using System;
using System.IO;
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

        private async void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            using var file = File.OpenWrite(@"errorlog.txt");
            using (var writer = new StreamWriter(file))
            {
                await writer.WriteLineAsync($"Exception ({DateTime.Now}): {e.Exception.Message}");
                await writer.WriteLineAsync(e.Exception.StackTrace);
            }

            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }
    }
}
