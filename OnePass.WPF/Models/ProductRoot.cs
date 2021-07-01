using System.Collections.Generic;

namespace OnePass.Models
{
    public class ProductRoot
    {
        public IEnumerable<Product> Products { get; set; }
    }
}
