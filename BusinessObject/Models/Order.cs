using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsCart { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? Freight { get; set; }
        public int? TableAdress { get; set; }
        public int? CustomerId { get; set; }
        public string? Note { get; set; }
        public bool? IsCheck { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
