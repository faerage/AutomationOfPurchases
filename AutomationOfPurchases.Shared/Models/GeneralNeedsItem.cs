using AutomationOfPurchases.Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class GeneralNeedsItem
{
    [Key]
    public int GeneralNeedsItemId { get; set; }

    public int GeneralNeedsListId { get; set; }
    [ForeignKey(nameof(GeneralNeedsListId))]
    public virtual GeneralNeedsList? List { get; set; }

    public int ItemId { get; set; }
    public virtual Item? Item { get; set; }

    public int Quantity { get; set; }

    // Для зручності можна зберігати відомості, хто замовляв
    public string? OrderedById { get; set; }
    public virtual AppUser? OrderedBy { get; set; }

    // За бажання - посилання на "оригінальний" RequestItem:
    public int? OriginalRequestItemId { get; set; }
    [ForeignKey(nameof(OriginalRequestItemId))]
    public virtual RequestItem? OriginalRequestItem { get; set; }
}