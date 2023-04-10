using OnePass.WPF.Models;
using OnePass.WPF.Services;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for VerifyWindow.xaml
    /// </summary>
    public partial class VerifyWindow : Window
    {
        private readonly ContentWindow _contentWindow;

        public VerifyWindow(ContentWindow contentWindow, VerifyModel model)
        {
            _contentWindow = contentWindow ?? throw new ArgumentNullException(nameof(contentWindow));
            InitializeComponent();
            Owner = contentWindow;
            DataContext = model;

            PasswordTextbox.Focus();
        }

        private void AddAccountButton_VerifyPassword(object sender, RoutedEventArgs e)
        {
            if (DataContext is VerifyModel model)
            {
                if (model.IsValid())
                {
                    var data = App.Current.GetService<UserData>();
                    if (data.Password == model.Password)
                    {
                        if (_contentWindow.DataContext is ContentModel contentModel)
                        {
                            ExportJSON(data, contentModel);
                        }
                    }
                    else
                    {
                        model.PasswordValidation = "Invalid Password";
                    }
                }
            }
        }

        private void ExportJSON(UserData data, ContentModel contentModel)
        {
            try
            {
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), $"export-{data.Username}.json");
                using var fileStream = new FileStream(filepath, FileMode.Create);
                JsonSerializer.SerializeAsync(fileStream, contentModel.Accounts, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                Close();
                MessageBox.Show(owner: this, $"Accounts data has been export to JSON file: {filepath}");
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
