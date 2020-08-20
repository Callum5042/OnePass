using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePass.Services
{
    public class ProductRoot
    {
        public IEnumerable<Product> Products { get; set; }
    }
}
