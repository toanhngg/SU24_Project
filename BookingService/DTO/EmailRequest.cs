namespace BookingService.DTO
{
    public class EmailRequest
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
