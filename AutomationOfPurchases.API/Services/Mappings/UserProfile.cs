using AutoMapper;
using AutomationOfPurchases.Shared.Models;
using AutomationOfPurchases.Shared.DTOs;

namespace AutomationOfPurchases.API.Services.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Мапінг для AppUser <-> UserDTO
            CreateMap<AppUser, UserDTO>()
                .ReverseMap();

            // Додаємо мапінг для Department <-> DepartmentDTO,
            // щоб AutoMapper міг коректно відобразити властивість Department у UserDTO
            CreateMap<Department, DepartmentDTO>()
                .ReverseMap();
        }
    }
}
