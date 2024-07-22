using BusinessObject.Models;
using System.Text.Json.Serialization;

namespace Project_API.DTO
{
    public class OrderDTO
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonIgnore]
        public bool Status { get; set; }
        [JsonIgnore]
        public DateTime OrderDate { get; set; }
        public decimal? Freight { get; set; }

        public int TableAdress { get; set; }
        [JsonIgnore]
        public bool? IsCheck { get; set; }
        public virtual CustomerDTO? Customer { get; set; }

        public virtual ICollection<OrderDetailDTO> OrderDetails { get; set; }
    }
}
