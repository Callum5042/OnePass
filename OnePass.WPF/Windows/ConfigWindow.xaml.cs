using OnePass.WPF.Infrastructure;
using OnePass.WPF.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();

            DataContext = new ConfigModel() 
            { 
                Lowercase = App.Current.AppOptions.Lowercase,
                Uppercase = App.Current.AppOptions.Uppercase,
                Numbers = App.Current.AppOptions.Numbers,
                Symbols = App.Current.AppOptions.Symbols,
                MinLength = App.Current.AppOptions.MinLength, 
                MaxLength = App.Current.AppOptions.MaxLength
            };
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private async void Button_Click_SaveConfig(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ConfigModel;

            if (!model.IsValid)
                return;

            // Update memory
            App.Current.AppOptions.Lowercase = model.Lowercase;
            App.Current.AppOptions.Uppercase = model.Uppercase;
            App.Current.AppOptions.Numbers = model.Numbers;
            App.Current.AppOptions.Symbols = model.Symbols;
            App.Current.AppOptions.MinLength = model.MinLength;
            App.Current.AppOptions.MaxLength = model.MaxLength;

            // Update file
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(Path.Combine(appdata, "OnePass"));
            var path = Path.Combine(appdata, @"OnePass", "options.json");

            using var file = File.Create(path);
            await JsonSerializer.SerializeAsync(file, App.Current.AppOptions);

            Close();
        }
    }
}
