using Microsoft.AspNetCore.Identity;

namespace AutomationOfPurchases.Shared.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        // НОВЕ: Зв’язок із DepartmentEconomist (many-to-many)
        public virtual ICollection<DepartmentEconomist> DepartmentEconomists { get; set; }
            = new List<DepartmentEconomist>();
    }
}
