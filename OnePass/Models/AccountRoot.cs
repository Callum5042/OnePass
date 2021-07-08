using System.Collections.Generic;

namespace OnePass.Models
{
    public class AccountRoot
    {
        public IList<Account> Accounts { get; protected set; } = new List<Account>();
    }
}
