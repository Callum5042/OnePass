using System.Collections.Generic;

namespace OnePass.Models
{
    public class RootAccount
    {
        public IList<Account> Accounts { get; set; } = new List<Account>();
    }
}
