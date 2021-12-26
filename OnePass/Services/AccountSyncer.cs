using OnePass.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnePass.Services
{
    public class AccountSyncer
    {
        public IEnumerable<Account> Sync(IList<Account> localAccounts, IList<Account> remoteAccounts)
        {
            if (localAccounts == null)
            {
                throw new ArgumentNullException(nameof(localAccounts));
            }

            if (remoteAccounts == null)
            {
                throw new ArgumentNullException(nameof(remoteAccounts));
            }

            // Join
            var jointAccounts = new List<Account>();

            // Enumerate local accounts
            foreach (var account in localAccounts)
            {
                var remoteAccount = remoteAccounts.FirstOrDefault(x => x.Id == account.Id);
                
                if (remoteAccount == null)
                {
                    // If account doesn't exist then we add it
                    jointAccounts.Add(account);
                }
                else
                {
                    if (account.DateModified >= remoteAccount.DateModified)
                    {
                        jointAccounts.Add(account);
                    }
                    else
                    {
                        jointAccounts.Add(remoteAccount);
                    }
                }
            }

            // Add remote accounts that are missing
            foreach (var account in remoteAccounts)
            {
                if (!jointAccounts.Any(x => x.Id == account.Id))
                {
                    jointAccounts.Add(account);
                }
            }

            return jointAccounts;
        }
    }
}
