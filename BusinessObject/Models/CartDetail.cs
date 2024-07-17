using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class CartDetail
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public virtual Cart Cart { get; set; } = null!;
    }
}
