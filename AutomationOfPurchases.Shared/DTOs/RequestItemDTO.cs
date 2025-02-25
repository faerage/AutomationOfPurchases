namespace AutomationOfPurchases.Shared.DTOs
{
    public class RequestItemDTO
    {
        public int RequestItemId { get; set; }

        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }

        public int Quantity { get; set; }
        public bool Satisfied { get; set; }

        public int? OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        // Повертаємо список DeliveryRequest
        public List<DeliveryRequestDTO> DeliveryRequests { get; set; } = new();

        // Якщо потрібно, щоб зчитувати ідентифікатор заявки, з якою зв’язано RequestItem
        public int? RequestId { get; set; }
    }
}
