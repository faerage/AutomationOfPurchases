using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    public class RequestItem
    {
        [Key]
        public int RequestItemId { get; set; }

        // Навігаційна властивість до товару з каталогу
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        public int Quantity { get; set; }
        public bool Satisfied { get; set; }

        // Хто замовив цей Item
        // Раніше: int? OrderedById
        public string? OrderedById { get; set; }
        [ForeignKey(nameof(OrderedById))]
        public virtual AppUser? OrderedBy { get; set; }

        // Для кожного RequestItem можна створити декілька DeliveryRequest
        public virtual ICollection<DeliveryRequest> DeliveryRequests { get; set; }
            = new List<DeliveryRequest>();

        // Якщо потрібний прямий зв’язок із Request (один-до-багатьох)
        public int? RequestId { get; set; }
        [ForeignKey(nameof(RequestId))]
        public virtual Request? Request { get; set; }
    }
}
