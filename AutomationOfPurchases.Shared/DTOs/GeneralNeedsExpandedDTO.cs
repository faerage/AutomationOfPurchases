public class GeneralNeedsListExpandedDTO
{
    public int ListId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? NullificationDate { get; set; }

    public List<GeneralAggregatedItemDTO> AggregatedItems { get; set; } = new();
}

public class GeneralAggregatedItemDTO
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = "";
    public string StorageUnit { get; set; } = "";
    public int TotalQuantity { get; set; }
    public bool IsExpanded { get; set; } = false;

    public List<GeneralDetailDTO> Details { get; set; } = new();
}

public class GeneralDetailDTO
{
    public int RequestItemId { get; set; }
    public string OrderedByFullName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public int Quantity { get; set; }
}
