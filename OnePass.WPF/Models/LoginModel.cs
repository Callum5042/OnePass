using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    public class LoginModel : ObservableObject
    {
        public LoginModel()
        {
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        public ICommand LoginCommand { get; }

        public ICommand RegisterCommand { get; }

        private void Login()
        {
            MessageBox.Show("Login");
        }

        private void Register()
        {

        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
