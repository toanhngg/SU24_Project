using System.Text.Json.Serialization;

namespace Project_API.DTO
{
    public class CustomerDTO
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }

    }
}
