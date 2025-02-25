namespace AutomationOfPurchases.Shared.DTOs
{
    public class DeliveryRequestDTO
    {
        public int DeliveryRequestId { get; set; }

        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }

        public int Quantity { get; set; }

        public int OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        public string Warehouse { get; set; } = string.Empty;

        public int? RequestItemId { get; set; }
        // Якщо потрібна інформація про RequestItem
        // public RequestItemDTO? RequestItem { get; set; }
    }
}
