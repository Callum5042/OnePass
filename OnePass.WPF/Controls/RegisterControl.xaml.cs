using OnePass.WPF.Models;
using OnePass.WPF.Windows;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OnePass.WPF.Controls
{
    /// <summary>
    /// Interaction logic for RegisterControl.xaml
    /// </summary>
    public partial class RegisterControl : UserControl, IWindowRegisterModel
    {
        public RegisterControl()
        {
            InitializeComponent();
            DataContext = new RegisterModel(this);
        }

        public void ShowLoginControl()
        {
            var window = App.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.LoginControl.Visibility = Visibility.Visible;
            window.RegisterControl.Visibility = Visibility.Collapsed;
        }

        private void TextboxPassword_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Disable copy & pasting on the password box
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
