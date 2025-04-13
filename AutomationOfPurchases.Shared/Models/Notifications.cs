using System.ComponentModel.DataAnnotations;

namespace AutomationOfPurchases.Shared.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string RecipientId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LinkUrl { get; set; }

        // ДОДАНЕ ПОЛЕ ДЛЯ КАТЕГОРІЇ / ТИПУ (наприклад "Important", "Rejected", "Approved", ...).
        public string? Category { get; set; }

        public int? RequestId { get; set; }
    }
}
