using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    public class DeliveryRequest
    {
        [Key]
        public int DeliveryRequestId { get; set; }

        // Який Item доставляти
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        public int Quantity { get; set; }

        // Хто ініціював доставку
        // Раніше було int OrderedById, тепер string? 
        public string? OrderedById { get; set; }
        [ForeignKey(nameof(OrderedById))]
        public virtual AppUser? OrderedBy { get; set; }

        public string Warehouse { get; set; } = string.Empty;

        // Зв'язок назад до RequestItem (якщо хочемо)
        public int? RequestItemId { get; set; }
        [ForeignKey(nameof(RequestItemId))]
        public virtual RequestItem? RequestItem { get; set; }
    }
}
