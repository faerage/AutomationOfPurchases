using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomationOfPurchases.Shared.Models
{
    public class RequestItem
    {
        [Key]
        public int RequestItemId { get; set; }

        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        public int Quantity { get; set; }
        public bool Satisfied { get; set; }

        public string? OrderedById { get; set; }
        [ForeignKey(nameof(OrderedById))]
        public virtual AppUser? OrderedBy { get; set; }

        public virtual ICollection<DeliveryRequest> DeliveryRequests { get; set; }
            = new List<DeliveryRequest>();

        public int? RequestId { get; set; }
        [ForeignKey(nameof(RequestId))]
        public virtual Request? Request { get; set; }

        public int DeliveredQuantity { get; set; }
        public int ToPurchaseQuantity { get; set; }
    }
}
