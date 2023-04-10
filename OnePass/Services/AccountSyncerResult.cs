using OnePass.Models;
using System;
using System.Collections.Generic;

namespace OnePass.Services
{
    public class AccountSyncerResult
    {
        public IList<Account> Accounts { get; set; } = new List<Account>();

        public IList<Guid> DeletedAccounts { get; set; } = new List<Guid>();
    }
}
