using System.ComponentModel.DataAnnotations;

namespace AutomationOfPurchases.Shared.Models
{
    public class NetNeedsList
    {
        [Key]
        public int ListId { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? NullificationDate { get; set; }

        public virtual ICollection<NetNeedsItem> Items { get; set; }
            = new List<NetNeedsItem>();
    }
}
