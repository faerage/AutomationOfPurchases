namespace AutomationOfPurchases.Shared.DTOs
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LinkUrl { get; set; }

        // Додаємо таке саме поле Category
        public string? Category { get; set; }

        public int? RequestId { get; set; }
    }
}
