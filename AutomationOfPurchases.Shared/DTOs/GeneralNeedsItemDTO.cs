namespace AutomationOfPurchases.Shared.DTOs
{
    /// <summary>
    /// Відповідає запису в таблиці GeneralNeedsItem 
    /// (копія даних для Загальних Потреб).
    /// </summary>
    public class GeneralNeedsItemDTO
    {
        public int GeneralNeedsItemId { get; set; }

        // Посилання на список GeneralNeedsList
        public int GeneralNeedsListId { get; set; }

        // Посилання на Item
        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }

        // Скільки потрібно (поза складом)
        public int Quantity { get; set; }

        // Хто був автором у оригінальній заявці
        public string? OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        // За бажання: щоб знати, з якого оригінального RequestItem це скопійовано
        public int? OriginalRequestItemId { get; set; }
        // Можна додати public RequestItemDTO? OriginalRequestItem { get; set; }
        // якщо потрібен детальніший зв’язок
    }
}
