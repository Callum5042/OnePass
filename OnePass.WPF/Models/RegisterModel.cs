using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OnePass.WPF.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    public class RegisterModel : ObservableValidator
    {
        public RegisterModel()
        {
            CreateAccountCommand = new RelayCommand(() => throw new System.NotImplementedException());
            BackCommand = new RelayCommand(Back);
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string PasswordRepeat { get; set; }

        public ICommand CreateAccountCommand { get; }

        public ICommand BackCommand { get; }

        public void Back()
        {
            // Might have to refactor this out into a service
            var window = App.Current.Windows.OfType<LoginWindow2>().FirstOrDefault();
            window.LoginControl.Visibility = Visibility.Visible;
            window.RegisterControl.Visibility = Visibility.Collapsed;
        }
    }
}
