namespace BookingService.DTO
{
    public class BookingUpdateDTO
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string? Time { get; set; }
        public int? NumberOfPeople { get; set; }
        public string? Note { get; set; }
        public DateTime? DateBooking { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateCheckOut { get; set; }
        public int? BookingTable { get; set; }
        public bool? IsCheck { get; set; }
      //  public int? UserCheck { get; set; }

    }
}
