using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    /// <summary>
    /// Зв’язка “багато-до-багатьох” між відділами та економістами
    /// </summary>
    public class DepartmentEconomist
    {
        // Ключ складається з двох полів (Composite Key): DepartmentId + EconomistId
        public int DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        public string EconomistId { get; set; } = string.Empty;
        [ForeignKey(nameof(EconomistId))]
        public virtual AppUser? Economist { get; set; }
    }
}
