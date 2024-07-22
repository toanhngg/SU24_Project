using System.Text.Json.Serialization;

namespace Project_API.DTO
{
    public class ReviewDTO
    {
        public string Comment { get; set; }
        [JsonIgnore]
        public int UserId { get; set; }
        public int ProductId { get; set; }
        [JsonIgnore]
        public DateTime DateModified { get; set; }
    }
}
