using OnePass.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnePass.Services
{
    public class AccountSyncer : IAccountSyncer
    {
        public AccountSyncerResult Sync(OnePassData data1, OnePassData data2)
        {
            var deletedGuids = data1.DeletedAccounts.Union(data2.DeletedAccounts).Distinct();

            var accounts = data1.Accounts.Union(data2.Accounts)
                .Where(x => !deletedGuids.Contains(x.Guid))
                .GroupBy(x => x.Guid)
                .Select(x => x.OrderByDescending(y => y.DateModified).First());

            // Apply history
            var accountPasswordHistory = MergeDuplicatePasswordHistory(data1, data2);
            foreach (var account in accounts)
            {
                if (accountPasswordHistory.TryGetValue(account.Guid, out var passwordHistory))
                {
                    account.PasswordHistory = passwordHistory;
                }
            }

            return new AccountSyncerResult()
            {
                Accounts = accounts.ToList(),
                DeletedAccounts = deletedGuids.ToList(),
            };
        }

        private IDictionary<Guid, IList<PasswordHistory>> MergeDuplicatePasswordHistory(OnePassData data1, OnePassData data2)
        {
            var history = data1.Accounts.Union(data2.Accounts).GroupBy(x => x.Guid, x => x.PasswordHistory);
            var distinctHistory = new Dictionary<Guid, IList<PasswordHistory>>();

            foreach (var account in history)
            {
                distinctHistory.Add(account.Key, account.SelectMany(x => x).GroupBy(x => x.Guid).Select(x => x.First()).ToList());
            }

            return distinctHistory;
        }
    }
}
