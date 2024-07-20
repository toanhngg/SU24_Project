namespace Project_API.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ContactName { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; }
    }

    public class UserUpdateDTO
    {
        public string? Phone { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ContactName { get; set; }
    }
}
