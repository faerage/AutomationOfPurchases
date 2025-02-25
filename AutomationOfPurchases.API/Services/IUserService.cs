using AutomationOfPurchases.Shared.DTOs;

namespace AutomationOfPurchases.API.Services
{
    public interface IUserService
    {
        Task<UserDTO?> GetByIdAsync(int id);
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO> CreateAsync(UserDTO dto);
        Task<bool> UpdateAsync(int id, UserDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
