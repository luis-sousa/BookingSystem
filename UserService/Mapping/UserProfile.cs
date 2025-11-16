using AutoMapper;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}
