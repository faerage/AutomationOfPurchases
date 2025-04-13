namespace AutomationOfPurchases.Shared.DTOs
{
    public class DepartmentDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        // Змінили тип з int? на string?
        public string? HeadOfDepartmentId { get; set; }
        public UserDTO? HeadOfDepartment { get; set; }

        public int FreeCapital { get; set; }

        // За потреби можна включити сюди список економістів
        // public List<UserDTO> Economists { get; set; } = new();
    }
}
