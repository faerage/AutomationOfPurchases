using AutomationOfPurchases.Shared.DTOs;

namespace AutomationOfPurchases.API.Services
{
    public interface IDepartmentHeadService
    {
        Task<List<RequestDTO>> GetRequestsInMyDepartmentAsync(string departmentHeadUserId);
        Task<bool> ApproveRequestAsync(int requestId, string departmentHeadUserId);
        Task<bool> RejectRequestAsync(int requestId, string departmentHeadUserId, string reason);
        Task<List<string>> GetEconomistsOfDepartment(AppDbContext context, int departmentId);
    }
}
