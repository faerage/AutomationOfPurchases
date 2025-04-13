namespace AutomationOfPurchases.Shared.DTOs
{
    /// <summary>
    /// Відповідає запису в таблиці NetNeedsItem
    /// (копія даних для Чистих Потреб).
    /// </summary>
    public class NetNeedsItemDTO
    {
        public int NetNeedsItemId { get; set; }

        // Посилання на список NetNeedsList
        public int NetNeedsListId { get; set; }

        // Посилання на Item
        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }

        // Скільки потрібно
        public int Quantity { get; set; }

        // Хто був автором у оригінальній заявці
        public string? OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        // За бажання: посилання на оригінал
        public int? OriginalRequestItemId { get; set; }
    }
}
