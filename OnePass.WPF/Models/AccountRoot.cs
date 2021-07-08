using System.Collections.Generic;

namespace OnePass.WPF.Models
{
    public class AccountRoot
    {
        public IList<Account> Accounts { get; set; } = new List<Account>();

        public string RememberUsername { get; set; }
    }
}
