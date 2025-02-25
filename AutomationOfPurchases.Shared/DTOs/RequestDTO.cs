using AutomationOfPurchases.Shared.Enums;

namespace AutomationOfPurchases.Shared.DTOs
{
    public class RequestDTO
    {
        public int RequestId { get; set; }
        public DateTime CreationDate { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Draft;

        public int OrderedById { get; set; }
        public UserDTO? OrderedBy { get; set; }

        public bool ApprovedByDepartmentHead { get; set; }
        public bool ApprovedByEconomist { get; set; }

        public bool ReportDepartmentHead { get; set; }
        public bool ReportRequester { get; set; }
        public bool ReportEconomist { get; set; }

        public List<RequestItemDTO> Items { get; set; } = new();
    }
}
