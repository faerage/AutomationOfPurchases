using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public string? HeadOfDepartmentId { get; set; }
        [ForeignKey(nameof(HeadOfDepartmentId))]
        public virtual AppUser? HeadOfDepartment { get; set; }

        public virtual ICollection<AppUser> Economists { get; set; }
            = new List<AppUser>();

        public int FreeCapital { get; set; }

        // НОВЕ: Зв’язок із DepartmentEconomist (many-to-many)
        public virtual ICollection<DepartmentEconomist> DepartmentEconomists { get; set; }
            = new List<DepartmentEconomist>();
    }
}
