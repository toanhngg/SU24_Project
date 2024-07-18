using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Booking
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public int? NumberOfPeople { get; set; }
        public string? Note { get; set; }
        public DateTime? DateBooking { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}
