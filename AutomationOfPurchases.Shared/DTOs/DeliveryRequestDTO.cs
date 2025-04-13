namespace AutomationOfPurchases.Shared.DTOs
{
    public class DeliveryRequestDTO
    {
        public int DeliveryRequestId { get; set; }

        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }

        public int Quantity { get; set; }

        public string? OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        public string Warehouse { get; set; } = string.Empty;

        public int? RequestItemId { get; set; }

        // Поля для відображення на фронті (DeliveryRequests.razor):
        public string ItemName { get; set; } = "";
        public string OrderedByFullName { get; set; } = "";
        public string OrderedByDepartmentName { get; set; } = "";
        public string OrderedByEmail { get; set; } = "";
    }
}
