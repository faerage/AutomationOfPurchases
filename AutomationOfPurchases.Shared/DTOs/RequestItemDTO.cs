namespace AutomationOfPurchases.Shared.DTOs
{
    /// <summary>
    /// Відповідає оригінальному рядку заявки.
    /// Зберігає інформацію про виконання (Delivered/ToPurchase).
    /// </summary>
    public class RequestItemDTO
    {
        public int RequestItemId { get; set; }

        // Посилання на заявку
        public int RequestId { get; set; }

        // Посилання на Item
        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }

        // Скільки замовили
        public int Quantity { get; set; }
        // Чи повністю задоволений цей рядок
        public bool Satisfied { get; set; }

        // Хто створив (для прив’язки до автора)
        public string? OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        // Показує, скільки вже фактично видано зі складу
        public int DeliveredQuantity { get; set; }
        // Показує, скільки відправлено на закупівлю
        public int ToPurchaseQuantity { get; set; }
    }
}
