using AutoMapper;
using AutomationOfPurchases.Shared.Models;
using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Enums;

namespace AutomationOfPurchases.API.Services.Mappings
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<Request, RequestDTO>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => ConvertToEnum(src.Status)))
                .ForMember(dest => dest.OrderedByFullName,
                    opt => opt.MapFrom(src => src.OrderedBy != null ? src.OrderedBy.FullName : ""))
                .ForMember(dest => dest.OrderedByEmail,
                    opt => opt.MapFrom(src => src.OrderedBy != null ? src.OrderedBy.Email : ""))
                .ForMember(dest => dest.OrderedByDepartmentName,
                    opt => opt.MapFrom(src => src.OrderedBy != null && src.OrderedBy.Department != null
                        ? src.OrderedBy.Department.DepartmentName
                        : ""))
                .ForMember(dest => dest.DepartmentHeadApproverFullName,
                    opt => opt.MapFrom(src => src.DepartmentHeadApprover != null
                        ? src.DepartmentHeadApprover.FullName
                        : null))
                .ForMember(dest => dest.EconomistApproverFullName,
                    opt => opt.MapFrom(src => src.EconomistApprover != null
                        ? src.EconomistApprover.FullName
                        : null))
                .ForMember(dest => dest.RejectedByUserFullName,
                    opt => opt.MapFrom(src => src.RejectedByUser != null
                        ? src.RejectedByUser.FullName
                        : null))
                // Явне мапування для колекції позицій (Items)
                .ForMember(dest => dest.Items,
                           opt => opt.MapFrom(src => src.Items))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.OrderedBy, opt => opt.Ignore())
                .ForMember(dest => dest.OrderedById, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentHeadApprover, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentHeadApproverId, opt => opt.Ignore())
                .ForMember(dest => dest.EconomistApprover, opt => opt.Ignore())
                .ForMember(dest => dest.EconomistApproverId, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.RequestId, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDate, opt => opt.Ignore());

            CreateMap<RequestItem, RequestItemDTO>().ReverseMap();
            CreateMap<Item, ItemDTO>().ReverseMap();
        }

        private RequestStatus ConvertToEnum(string statusString)
        {
            return statusString.ToLower() switch
            {
                "draft" => RequestStatus.Draft,
                "pendingdepartmenthead" => RequestStatus.PendingDepartmentHead,
                "pendingeconomist" => RequestStatus.PendingEconomist,
                "approved" => RequestStatus.Approved,
                "rejected" => RequestStatus.Rejected,
                _ => RequestStatus.Draft
            };
        }
    }
}
