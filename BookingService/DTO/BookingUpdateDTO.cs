using System.Text.Json.Serialization;

namespace BookingService.DTO
{
    public class BookingUpdateDTO
    {
        public int Id { get; set; }
        public string? Note { get; set; } = null!;
        public DateTime? DateStart { get; set; } = null!;
        public DateTime? DateCheckOut { get; set; } = null!;
        public bool? IsCheck { get; set; } = null!;
        [JsonIgnore]
        public int? UserCheck { get; set; }

    }
}
