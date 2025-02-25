using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        // Керівник відділу. 
        // Було int? HeadOfDepartmentId, але AppUser.Id -> string
        public string? HeadOfDepartmentId { get; set; }

        [ForeignKey(nameof(HeadOfDepartmentId))]
        public virtual AppUser? HeadOfDepartment { get; set; }

        // Список економістів (посилаються на DepartmentId в AppUser)
        public virtual ICollection<AppUser> Economists { get; set; }
            = new List<AppUser>();

        public int FreeCapital { get; set; }
    }
}
