using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class OrderDetail
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool? Status { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
