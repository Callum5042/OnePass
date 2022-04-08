using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnePass.WPF.Models
{
    public class LoginModel : ObservableValidator
    {
        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        public static string UsernameLabel => "Username";

        public static string PasswordLabel => "Password";
    }
}
