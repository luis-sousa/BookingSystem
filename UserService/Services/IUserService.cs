using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserDto dto);
        Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);
        Task<UserDto?> PatchAsync(int id, JsonPatchDocument<UpdateUserDto> patch);
        Task<bool> DeleteAsync(int id);
        Task<AuthDto?> LoginAsync(string email, string password);
        Task<bool> UpdatePasswordAsync(int id, UpdatePasswordDto dto);
    }
}
