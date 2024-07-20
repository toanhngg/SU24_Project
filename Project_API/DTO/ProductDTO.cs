namespace Project_API.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public double Weight { get; set; }
        public string? Image { get; set; }
        public int? Ammount { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public IFormFile? ImageFile { get; set; }

    }
}
