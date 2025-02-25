using AutomationOfPurchases.API.Repositories;
using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using AutoMapper;

namespace AutomationOfPurchases.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return null;

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> CreateAsync(UserDTO dto)
        {
            // DTO -> Entity
            var entity = _mapper.Map<AppUser>(dto);

            await _unitOfWork.Users.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // Зберігаємо, щоб отримати новий Id

            // Повторний мап: Entity -> DTO з оновленим UserId
            return _mapper.Map<UserDTO>(entity);
        }

        public async Task<bool> UpdateAsync(int id, UserDTO dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            // DTO -> (оновлюємо) user
            _mapper.Map(dto, user);

            // EF відслідковує user, тому можна просто викликати UpdateAsync
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
