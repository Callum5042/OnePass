using OnePass.Models;
using System.Collections.Generic;

namespace OnePass.Services
{
    public interface IAccountSyncer
    {
        AccountSyncerResult Sync(OnePassData data1, OnePassData data2);
    }
}