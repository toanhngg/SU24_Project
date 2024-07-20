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

        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public decimal? Freight { get; set; }

        public int TableAdress { get; set; }
        [JsonIgnore]
        public bool? IsCart { get; set; }
        public virtual CustomerDTO? Customer { get; set; }

        public virtual ICollection<OrderDetailDTO> OrderDetails { get; set; }
    }
}
