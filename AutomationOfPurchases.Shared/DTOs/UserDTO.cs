using AutomationOfPurchases.Shared.Enums;

namespace AutomationOfPurchases.Shared.DTOs
{
    public class UserDTO
    {
        public string? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public DepartmentDTO? Department { get; set; }

        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.None;
    }
}
