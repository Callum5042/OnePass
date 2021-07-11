using OnePass.WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Account = OnePass.Models.Account;

namespace OnePass.Handlers.Interfaces
{
    public interface IDeleteProductHandler
    {
        Task<IEnumerable<Account>> DeleteProductAsync(AccountViewModel model);
    }
}