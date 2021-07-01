using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OnePass.Models
{
    public class Product : INotifyPropertyChanged
    {
        private string name;
        private string login;
        private string password;

        public int Id { get; set; }

        public string Name
        {
            get => name;
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Login 
        { 
            get => login; 
            set
            {
                if (value != login)
                {
                    login = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Password 
        { 
            get => password; 
            set
            {
                if (value != password)
                {
                    password = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(CensoredPassword));
                }
            }
        }

        public string CensoredPassword => new string('*', Password.Length);

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
