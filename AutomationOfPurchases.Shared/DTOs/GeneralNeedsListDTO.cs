namespace AutomationOfPurchases.Shared.DTOs
{
    public class GeneralNeedsListDTO
    {
        public int ListId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? NullificationDate { get; set; }

        public List<RequestItemDTO> Items { get; set; } = new();
    }
}