using System.ComponentModel;

namespace OnePass.Services
{
    public class Product
    {
        public int Id { get; protected set; }

        public string Name { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string CensoredPassword => new string('*', Password.Length);
    }
}
