using System.ComponentModel.DataAnnotations;

namespace AutomationOfPurchases.Shared.Models
{
    public class GeneralNeedsList
    {
        [Key]
        public int ListId { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? NullificationDate { get; set; }

        // Припустимо, що маємо список RequestItem, який пов'язуємо через проміжну таблицю
        // або ключ із RequestItem. Тут показано підхід "один-до-багатьох" для прикладу:
        public virtual ICollection<RequestItem> Items { get; set; }
            = new List<RequestItem>();
    }
}
