﻿namespace Project_API.DTO
{
    public class UserRegister
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactName { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; }
    }
}
