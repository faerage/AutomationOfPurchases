using AutoMapper;
using AutomationOfPurchases.Shared.Models;
using AutomationOfPurchases.Shared.DTOs;

namespace AutomationOfPurchases.API.Services.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Відображення User -> UserDTO
            CreateMap<AppUser, UserDTO>()
                // За потреби можна налаштувати додаткові ForMember
                .ReverseMap();
            // ReverseMap() робить і зворотне мапування (UserDTO -> User)
        }
    }
}
