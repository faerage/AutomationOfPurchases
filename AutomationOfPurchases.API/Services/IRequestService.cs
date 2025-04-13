using AutomationOfPurchases.Shared.DTOs;

namespace AutomationOfPurchases.API.Services
{
    public interface IRequestService
    {
        Task<RequestDTO> CreateRequestAsync(RequestDTO requestDto, string orderedByUserId);
        Task<List<RequestDTO>> GetRequestsByUserAsync(string userId);

        // Для перегляду заявки
        Task<RequestDTO?> GetRequestByIdAsync(int requestId, string currentUserId);
        Task<RequestDTO?> GetRequestByIdForUserAsync(int requestId, string userId);

        // Додавали раніше для видалення чернетки
        Task<bool> DeleteDraftAsync(int requestId, string userId);

        // ==================== ДОДАТИ НОВИЙ МЕТОД ====================
        // Для оновлення (PUT) існуючої заявки (включно з чернеткою).
        Task<RequestDTO?> UpdateRequestAsync(int requestId, RequestDTO requestDto, string userId);
    }
}
