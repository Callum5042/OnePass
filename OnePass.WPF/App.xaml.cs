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
        private readonly ServiceBuilder _serviceBuilder = new();

        public App()
        {
        }

        public T GetService<T>()
        {
            return _serviceBuilder.GetService<T>();
        }

        private async void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            using var file = File.OpenWrite(@"errorlog.txt");
            using (var writer = new StreamWriter(file))
            {
                await writer.WriteLineAsync($"Exception ({DateTime.Now}): {e.Exception.Message}");
                await writer.WriteLineAsync(e.Exception.StackTrace);
            }

            Environment.Exit(0);
        }

        public new static App Current => (App)Application.Current;

        public string Filename => $"{Username}.bin";

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
