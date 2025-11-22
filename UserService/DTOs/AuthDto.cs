namespace UserService.DTOs
{
    public class AuthDto
    {
        public required UserDto User { get; set; }
        public required string Token { get; set; }
    }
}
