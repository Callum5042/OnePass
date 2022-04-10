using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    public interface IWindowRegisterModel
    {
        void ShowLoginControl();
    }

    public class RegisterModel : ObservableValidator
    {
        private readonly IWindowRegisterModel _panel;

        public RegisterModel(IWindowRegisterModel panel)
        {
            _panel = panel;

            CreateAccountCommand = new AsyncRelayCommand(CreateAccount);
            BackCommand = new RelayCommand(Back);

            ErrorsChanged += LoginModel_ErrorsChanged;
        }

        private void LoginModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Username))
            {
                UsernameValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            }

            if (e.PropertyName == nameof(Password))
            {
                PasswordValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            }

            if (e.PropertyName == nameof(PasswordRepeat))
            {
                PasswordRepeatValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            }
        }

        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = $"{nameof(Password)} must be at least 10 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "The Repeat Password field is required.")]
        [MinLength(10, ErrorMessage = $"{nameof(Password)} must be at least 10 characters.")]
        public string PasswordRepeat { get; set; }

        public ICommand CreateAccountCommand { get; }

        public ICommand BackCommand { get; }

        public async Task CreateAccount()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                return;
            }

            // Check if file already exists
            if (File.Exists($"{Username}.bin"))
            {
                MessageBox.Show("File already exists");
                return;
            }

            // Create file
            MessageBox.Show("Not yet implemented");
            // throw new System.NotImplementedException();
            //using var file = File.Open($"{Username}.bin", FileMode.CreateNew);
            //await JsonSerializer.SerializeAsync(file, new AppOptions());
        }

        public void Back()
        {
            _panel.ShowLoginControl();
        }

        private string _usernameValidationMessage;

        public string UsernameValidationMessage
        {
            get => _usernameValidationMessage;
            set => SetProperty(ref _usernameValidationMessage, value);
        }

        private string _passwordValidationMessage;

        public string PasswordValidationMessage
        {
            get => _passwordValidationMessage;
            set => SetProperty(ref _passwordValidationMessage, value);
        }

        private string _passwordRepeatValidationMessage;

        public string PasswordRepeatValidationMessage
        {
            get => _passwordRepeatValidationMessage;
            set => SetProperty(ref _passwordRepeatValidationMessage, value);
        }
    }
}
