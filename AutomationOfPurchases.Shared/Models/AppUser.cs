using Microsoft.AspNetCore.Identity;

namespace AutomationOfPurchases.Shared.Models
{
    // IdentityUser<string> за замовчуванням має ключ типу string (GUID).
    // Можна зробити <int> якщо треба.
    public class AppUser : IdentityUser
    {
        // Приклад додаткових полів, що були у Вашому "User"
        public string FullName { get; set; } = string.Empty;

        // За потреби – зв’язок із Department
        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        // Додайте інші поля, якщо бажаєте
        // Напр., ApprovedByEconomist, WarehouseId тощо
    }
}
