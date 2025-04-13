using AutomationOfPurchases.Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutomationOfPurchases.Shared.Models
{
    public class Request
    {
        [Key]
        public int RequestId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }

        public string Status { get; set; } = string.Empty;

        // Хто створив заявку
        public string? OrderedById { get; set; }
        [ForeignKey(nameof(OrderedById))]
        public virtual AppUser? OrderedBy { get; set; }

        public bool ApprovedByDepartmentHead { get; set; }
        public bool ApprovedByEconomist { get; set; }

        public bool ReportDepartmentHead { get; set; }
        public bool ReportRequester { get; set; }
        public bool ReportEconomist { get; set; }

        public virtual ICollection<RequestItem> Items { get; set; }
            = new List<RequestItem>();

        // ===================== НОВІ ПОЛЯ для відображення, ким затверджено =====================
        public string? DepartmentHeadApproverId { get; set; }
        [ForeignKey(nameof(DepartmentHeadApproverId))]
        public virtual AppUser? DepartmentHeadApprover { get; set; }

        public string? EconomistApproverId { get; set; }
        [ForeignKey(nameof(EconomistApproverId))]
        public virtual AppUser? EconomistApprover { get; set; }

        // ===================== Для відхилення =====================
        public string? RejectedByUserId { get; set; }
        [ForeignKey(nameof(RejectedByUserId))]
        public virtual AppUser? RejectedByUser { get; set; }

        public string? RejectionReason { get; set; }
    }
}