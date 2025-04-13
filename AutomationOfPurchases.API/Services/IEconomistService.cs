using AutomationOfPurchases.Shared.DTOs;

namespace AutomationOfPurchases.API.Services
{
    public interface IEconomistService
    {
        /// <summary>
        /// Повертає всі заявки користувачів відділів, з якими пов’язаний економіст
        /// (крім статусу Draft, якщо потрібно).
        /// </summary>
        Task<List<RequestDTO>> GetRequestsInMyDepartmentAsync(string economistUserId);

        /// <summary>
        /// Затвердження заявки економістом.
        /// </summary>
        Task<bool> ApproveRequestAsync(int requestId, string economistUserId);

        /// <summary>
        /// Відхилення заявки економістом із причиною.
        /// </summary>
        Task<bool> RejectRequestAsync(int requestId, string economistUserId, string reason);

        /// <summary>
        /// Перевіряє, чи закріплений користувач з economistUserId як економіст
        /// за департаментом із ідентифікатором departmentId.
        /// </summary>
        Task<bool> IsEconomistOfDepartmentAsync(int departmentId, string economistUserId);





    }
}
