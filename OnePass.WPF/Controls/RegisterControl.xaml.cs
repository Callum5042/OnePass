using OnePass.WPF.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OnePass.WPF.Controls
{
    /// <summary>
    /// Interaction logic for RegisterControl.xaml
    /// </summary>
    public partial class RegisterControl : UserControl
    {
        public RegisterControl()
        {
            InitializeComponent();
        }

        private void TextboxPassword_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Disable copy & pasting on the password box
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextboxUsername.Focus();
        }
    }
}
