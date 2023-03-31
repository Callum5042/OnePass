using Microsoft.Toolkit.Mvvm.ComponentModel;
using OnePass.Infrastructure;

namespace OnePass.WPF.Models
{
    [Inject]
    public class ConfigModel : ObservableValidator
    {
        public bool Lowercase { get; set; }

        public bool Uppercase { get; set; }

        public bool Numbers { get; set; }

        public bool Symbols { get; set; }

        public int MinLength { get; set; }

        public int MaxLength { get; set; }

        public bool IsValid
        {
            get
            {
                if (MinLength > MaxLength)
                {
                    ValidationMessage = "Min Length cannot be bigger than Max Length";
                    return false;
                }

                if (!Lowercase && !Uppercase && !Numbers && !Symbols)
                {
                    ValidationMessage = "Cannot generate password with all settings to off";
                    return false;
                }

                if (MaxLength > 100)
                {
                    ValidationMessage = "Cannot generate passwords with a max length greater than 100";
                    return false;
                }

                return true;
            }
        }

        public string ValidationMessage { get => validationMessage; private set => SetProperty(ref validationMessage, value); }
        private string validationMessage;
    }
}
