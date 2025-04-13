public class NetNeedsListExpandedDTO
{
    public int ListId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? NullificationDate { get; set; }

    public List<NetAggregatedItemDTO> AggregatedItems { get; set; } = new();
}

public class NetAggregatedItemDTO
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = "";
    public string StorageUnit { get; set; } = "";
    public int TotalQuantity { get; set; }
    public bool IsExpanded { get; set; } = false;

    public List<NetDetailDTO> Details { get; set; } = new();
}

public class NetDetailDTO
{
    public int RequestItemId { get; set; }
    public string OrderedByFullName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public int Quantity { get; set; }
}
