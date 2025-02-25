using System.ComponentModel.DataAnnotations;

namespace AutomationOfPurchases.Shared.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string StorageUnit { get; set; } = string.Empty;
    }
}
