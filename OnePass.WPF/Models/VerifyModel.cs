using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OnePass.WPF.Models
{
    public class VerifyModel : ObservableValidator
    {
        public VerifyModel()
        {
            ErrorsChanged += OnErrorsChanged;
        }

        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters.")]
        public string Password { get => password; set => SetProperty(ref password, value); }
        private string password;

        public string PasswordValidation { get => passwordValidation; set => SetProperty(ref passwordValidation, value); }
        private string passwordValidation;

        public string ButtonText { get => buttonText; set => SetProperty(ref buttonText, value); }
        private string buttonText;

        public bool IsValid()
        {
            ValidateAllProperties();
            return !HasErrors;
        }

        private void OnErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var error = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
            var validationLabel = GetType().GetProperties().FirstOrDefault(x => x.Name == $"{e.PropertyName}Validation");
            validationLabel?.SetValue(this, error);
        }
    }
}
