using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Enums;

public class RequestDTO
{
    public int RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    public string? OrderedById { get; set; }
    public string? OrderedByFullName { get; set; }
    public string? OrderedByEmail { get; set; }

    // НОВЕ ПОЛЕ:
    public string? OrderedByDepartmentName { get; set; }

    public bool ApprovedByDepartmentHead { get; set; }
    public bool ApprovedByEconomist { get; set; }

    public bool CanApproveAsEconomist { get; set; }

    public bool ReportDepartmentHead { get; set; }
    public bool ReportRequester { get; set; }
    public bool ReportEconomist { get; set; }

    public List<RequestItemDTO> Items { get; set; } = new();

    // Хто затвердив
    public string? DepartmentHeadApproverId { get; set; }
    public string? DepartmentHeadApproverFullName { get; set; }
    public string? EconomistApproverId { get; set; }
    public string? EconomistApproverFullName { get; set; }

    // Хто відхилив
    public string? RejectedByUserId { get; set; }
    public string? RejectedByUserFullName { get; set; }
    public string? RejectionReason { get; set; }
}
