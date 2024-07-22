using System;
using System.Collections.Generic;

namespace Project_Client.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public bool Status { get; set; }
        public decimal Freight { get; set; }
        public int TableAddress { get; set; }
        public Customer? CustomerId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public class OrderDetail
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public bool Check { get; set; } // Property to indicate if this is the first submission
        }

        public class Customer
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
    }
}
