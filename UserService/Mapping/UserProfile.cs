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
            CreateMap<CreateUserDto, User>()
    .ForMember(dest => dest.Role, opt => opt.Ignore()); // se Role não vem do DTO
        }
    }
}
