namespace AutomationOfPurchases.Shared.DTOs
{
    // Відповідає згрупованим потребам (GroupBy по ItemId)
    public class GroupedNeedDTO
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? StorageUnit { get; set; }
        public int TotalQuantity { get; set; }
        public bool IsExpanded { get; set; } = false;

        public List<DepartmentRequestDTO> Requests { get; set; } = new();
        // IsExpanded зазвичай тільки для клієнта (Blazor); 
        // можна зберегти тут, або не передавати на API.
    }

    public class DepartmentRequestDTO
    {
        public int RequestItemId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string OrderedByFullName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public string? OrderedById { get; set; }
    }

    public class SatisfyNeedModel
    {
        public int GeneralNeedsItemId { get; set; }

        public int QuantityToSatisfy { get; set; }
        public string WarehouseName { get; set; } = "";

        public bool IsNotAvailable { get; set; } = false;
    }

}
