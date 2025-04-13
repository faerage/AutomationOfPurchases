using AutomationOfPurchases.Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class NetNeedsItem
{
    [Key]
    public int NetNeedsItemId { get; set; }

    public int NetNeedsListId { get; set; }
    [ForeignKey(nameof(NetNeedsListId))]
    public virtual NetNeedsList? List { get; set; }

    public int ItemId { get; set; }
    public virtual Item? Item { get; set; }

    public int Quantity { get; set; }

    // Хто замовляв
    public string? OrderedById { get; set; }
    public virtual AppUser? OrderedBy { get; set; }

    // Посилання на оригінальний рядок (якщо потрібно)
    public int? OriginalRequestItemId { get; set; }
    [ForeignKey(nameof(OriginalRequestItemId))]
    public virtual RequestItem? OriginalRequestItem { get; set; }
}