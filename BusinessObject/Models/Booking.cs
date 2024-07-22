using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Booking
    {
        public Booking()
        {
            Tables = new HashSet<Table>();
        }

        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public int? NumberOfPeople { get; set; }
        public string? Note { get; set; }
        public DateTime? DateBooking { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateCheckOut { get; set; }
        public bool? IsCheck { get; set; }
        public int? UserCheck { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual User? UserCheckNavigation { get; set; }

        public virtual ICollection<Table> Tables { get; set; }
    }
}
