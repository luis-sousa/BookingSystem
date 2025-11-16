namespace UserService.DTOs
{
    public class UserDto
    {
        public int IdUser { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
    }
}
