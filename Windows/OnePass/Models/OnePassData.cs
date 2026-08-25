using System;
using System.Collections.Generic;

namespace OnePass.Models
{
    public class OnePassData
    {
        public IList<Account> Accounts { get; set; } = new List<Account>();

        public IList<Guid> DeletedAccounts { get; set; } = new List<Guid>();
    }
}
