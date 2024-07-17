using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Cart
    {
        public Cart()
        {
            CartDetails = new HashSet<CartDetail>();
        }

        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? Freight { get; set; }

        public virtual ICollection<CartDetail> CartDetails { get; set; }
    }
}
