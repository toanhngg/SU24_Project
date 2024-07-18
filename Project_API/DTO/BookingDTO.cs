namespace Project_API.DTO
{
    public class BookingDTO
    {
       // public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public int? NumberOfPeople { get; set; }
        public string? Note { get; set; }
    }
}
