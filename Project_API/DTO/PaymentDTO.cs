namespace Project_API.DTO
{
    public class PaymentDTO
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string Provider {  get; set; }  
    }
}
