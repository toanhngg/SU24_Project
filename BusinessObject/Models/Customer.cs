using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Bookings = new HashSet<Booking>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
