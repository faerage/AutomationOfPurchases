namespace AutomationOfPurchases.Shared.DTOs
{
    public class NetNeedsListDTO
    {
        public int ListId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? NullificationDate { get; set; }

        public List<NetNeedsItemDTO> Items { get; set; } = new();
    }
}
