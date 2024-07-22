
using BookingService.DTO;

public class BookingDTO
{
    // public int Id { get; set; }
    public CustomerDTO Customer { get; set; }
    public DateTime? Date { get; set; }
    public string Time { get; set; }  // Change to string
    public int? NumberOfPeople { get; set; }
    public string? Note { get; set; }

}
