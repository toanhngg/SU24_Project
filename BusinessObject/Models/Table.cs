using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Table
    {
        public Table()
        {
            Bookings = new HashSet<Booking>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Position { get; set; }
        public int? NumberOfPeople { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
