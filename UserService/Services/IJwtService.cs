using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface IJwtService
    {
        public string GenerateToken(UserDto user, string role);

    }
}
