using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OnePass.Infrastructure;
using OnePass.Services;
using OnePass.Windows;
using OnePass.WPF.Windows;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    [Inject]
    public class LoginModel : ObservableValidator
    {
        private readonly OnePassRepository _onePassRepository;

        public LoginModel(OnePassRepository onePassRepository)
        {
            _onePassRepository = onePassRepository;

            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);

            ErrorsChanged += LoginModel_ErrorsChanged;
        }

        private void LoginModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Username))
            {
                UsernameValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
                UsernameLabelVisibility = GetErrors(e.PropertyName).Any() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (e.PropertyName == nameof(Password))
            {
                PasswordValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
                PasswordLabelVisibility = GetErrors(e.PropertyName).Any() ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ICommand LoginCommand { get; }

        public ICommand RegisterCommand { get; }

        private void Login()
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                // We have validation errors so exit early
                return;
            }

            // Check file
            _onePassRepository.Username = Username;
            _onePassRepository.Filename = $"{Username}.bin";
            _onePassRepository.MasterPassword = Password;

            // Open main window
            var window = new MainWindow();
            window.Show();


            // Close login window
            var loginWindow = App.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            loginWindow.Close();

            //if (!File.Exists($"{Username}.bin"))
            //{
            //    MessageBox.Show("Boom");
            //}
        }

        private void Register()
        {
            // Might have to refactor this out into a service
            var window = App.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            window.LoginControl.Visibility = Visibility.Collapsed;
            window.RegisterControl.Visibility = Visibility.Visible;
        }

        private string _username;

        [Required]
        public string Username 
        {
            get => _username;
            set => SetProperty(ref _username, value, validate: true);
        }

        private string _password;

        [Required]
        [MinLength(10, ErrorMessage = $"{nameof(Password)} must be at least 10 characters.")]
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value, validate: true);
        }

        private string _usernameValidationMessage;

        public string UsernameValidationMessage 
        {
            get => _usernameValidationMessage;
            set => SetProperty(ref _usernameValidationMessage, value);
        }

        private Visibility _visibilityUsernameLabel = Visibility.Collapsed;

        public Visibility UsernameLabelVisibility 
        {
            get => _visibilityUsernameLabel;
            set => SetProperty(ref _visibilityUsernameLabel, value);
        }

        private string _passwordValidationMessage;

        public string PasswordValidationMessage
        {
            get => _passwordValidationMessage;
            set => SetProperty(ref _passwordValidationMessage, value);
        }

        private Visibility _visibilitypasswordLabel = Visibility.Collapsed;

        public Visibility PasswordLabelVisibility
        {
            get => _visibilitypasswordLabel;
            set => SetProperty(ref _visibilitypasswordLabel, value);
        }
    }
}
