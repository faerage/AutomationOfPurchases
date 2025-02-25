using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    public class Request
    {
        [Key]
        public int RequestId { get; set; }

        public DateTime CreationDate { get; set; }

        public string Status { get; set; } = string.Empty;

        // Хто створив заявку
        // Раніше: int OrderedById
        public string? OrderedById { get; set; }
        [ForeignKey(nameof(OrderedById))]
        public virtual AppUser? OrderedBy { get; set; }

        // Відмітки про затвердження
        public bool ApprovedByDepartmentHead { get; set; }
        public bool ApprovedByEconomist { get; set; }

        // Чи надсилалися повідомлення
        public bool ReportDepartmentHead { get; set; }
        public bool ReportRequester { get; set; }
        public bool ReportEconomist { get; set; }

        // Список замовлених ТМЦ (композиція: RequestItem'и належать цій заявці)
        public virtual ICollection<RequestItem> Items { get; set; }
            = new List<RequestItem>();
    }
}
