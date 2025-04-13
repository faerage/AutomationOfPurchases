using AutomationOfPurchases.API.Repositories;
using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Services
{
    /// <summary>
    /// Сервіс, що містить логіку для економіста:
    ///  - перегляд заявок свого відділу,
    ///  - затвердження/відхилення,
    ///  - додавання копій позицій заявки у GeneralNeedsList.
    /// </summary>
    public class EconomistService : IEconomistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public EconomistService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        /// <summary>
        /// Повертає всі заявки (крім чернеток) користувачів, чиї департаменти
        /// входять до списку департаментів, закріплених за economistUserId.
        /// </summary>
        public async Task<List<RequestDTO>> GetRequestsInMyDepartmentAsync(string economistUserId)
        {
            // 1) Які департаменти прив’язані до цього економіста
            var deptEconomistLinks = await _unitOfWork.Repository<DepartmentEconomist>()
                .FindAsync(de => de.EconomistId == economistUserId);
            var allowedDeptIds = deptEconomistLinks.Select(de => de.DepartmentId).ToList();

            if (!allowedDeptIds.Any())
                return new List<RequestDTO>();

            // 2) Завантажуємо всі заявки (окрім Draft)
            var allRequests = await _context.Requests
                .Include(r => r.OrderedBy).ThenInclude(u => u.Department)
                .Where(r => r.OrderedById != null && r.Status != "Draft")
                .ToListAsync();

            // 3) Фільтруємо: лишаємо ті, де відділ автора входить до allowedDeptIds
            var filtered = allRequests.Where(r =>
            {
                var deptId = r.OrderedBy?.DepartmentId;
                return deptId != null && allowedDeptIds.Contains(deptId.Value);
            }).ToList();

            // 4) Мапимо в DTO
            return filtered.Select(r => _mapper.Map<RequestDTO>(r)).ToList();
        }

        /// <summary>
        /// Затверджує заявку економістом:
        /// 1) Перевіряє, чи цей economistUserId справді закріплений за відділом заявки.
        /// 2) Ставить статус "Approved", виставляє ApprovedByEconomist = true.
        /// 3) Додає копії позицій у активний GeneralNeedsList (GeneralNeedsItem).
        /// 4) Створює повідомлення автору.
        /// </summary>
        public async Task<bool> ApproveRequestAsync(int requestId, string economistUserId)
        {
            // 1) Знаходимо заявку
            var request = await _unitOfWork.Requests.GetByIdAsync(requestId);
            if (request == null)
                return false;

            // 2) Перевіряємо, чи має economistUserId доступ до цього відділу
            if (!await IsRequestInMyDepartments(request, economistUserId))
                return false; // доступ заборонено

            // 3) Оновлюємо статус
            request.Status = "Approved";
            request.ApprovedByEconomist = true;
            request.EconomistApproverId = economistUserId;
            request.RejectedByUserId = null;
            request.RejectionReason = null;

            // 4) Копіюємо позиції заявки в активний GeneralNeedsList
            await AddItemsToGeneralNeedsListAsync(request.RequestId);

            // 5) Зберігаємо
            await _unitOfWork.Requests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // 6) Надсилаємо повідомлення автору заявки
            if (!string.IsNullOrEmpty(request.OrderedById))
            {
                var notif = new Notification
                {
                    RecipientId = request.OrderedById,
                    Title = "Економіст затвердив вашу заявку",
                    Message = $"Ваша заявка \"{request.Title}\" успішно затверджена економістом.",
                    LinkUrl = $"/request-details/{request.RequestId}",
                    Category = "Approved",
                    RequestId = request.RequestId
                };
                _context.Notifications.Add(notif);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        /// <summary>
        /// Відхиляє заявку економістом: ставить статус "Rejected",
        /// створює повідомлення автору (і керівнику, якщо треба).
        /// </summary>
        public async Task<bool> RejectRequestAsync(int requestId, string economistUserId, string reason)
        {
            // 1) Шукаємо заявку
            var request = await _unitOfWork.Requests.GetByIdAsync(requestId);
            if (request == null)
                return false;

            // 2) Перевіряємо доступ
            if (!await IsRequestInMyDepartments(request, economistUserId))
                return false;

            // 3) Ставимо статус "Rejected"
            request.Status = "Rejected";
            request.ApprovedByEconomist = false;
            request.RejectedByUserId = economistUserId;
            request.RejectionReason = reason;

            await _unitOfWork.Requests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // 4) Повідомлення автору
            if (!string.IsNullOrEmpty(request.OrderedById))
            {
                var notifAuthor = new Notification
                {
                    RecipientId = request.OrderedById,
                    Title = "Економіст відхилив вашу заявку",
                    Message = $"Заявку \"{request.Title}\" було відхилено економістом. Причина: {reason}",
                    LinkUrl = $"/request-details/{request.RequestId}",
                    Category = "Rejected",
                    RequestId = request.RequestId
                };
                _context.Notifications.Add(notifAuthor);
                await _context.SaveChangesAsync();
            }

            // 5) Якщо вона була схвалена керівником, можна повідомити керівника, 
            // але це опційно (подивіться ваші вимоги)

            return true;
        }

        // ==========================================================================
        // Допоміжні методи
        // ==========================================================================

        /// <summary>
        /// Копіює всі позиції (RequestItem) із заявки (requestId) 
        /// в активний GeneralNeedsList у вигляді (GeneralNeedsItem).
        /// </summary>
        private async Task AddItemsToGeneralNeedsListAsync(int requestId)
        {
            var request = await _unitOfWork.Requests.GetByIdAsync(requestId);

            // 1) Знайти активний GeneralNeedsList
            var activeList = await _context.GeneralNeedsLists
                .Where(l => l.NullificationDate == null)
                .FirstOrDefaultAsync();
            if (activeList == null)
            {
                // Якщо немає активного списку - створюємо
                activeList = new GeneralNeedsList
                {
                    CreationDate = DateTime.UtcNow
                };
                _context.GeneralNeedsLists.Add(activeList);
                await _context.SaveChangesAsync();
            }

            // 2) Знаходимо всі RequestItem оригіналу
            var requestItems = await _context.RequestItems
                .Where(ri => ri.RequestId == requestId)
                .ToListAsync();

            // 3) Створюємо GeneralNeedsItem для кожного
            foreach (var item in requestItems)
            {
                var gni = new GeneralNeedsItem
                {
                    GeneralNeedsListId = activeList.ListId,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    OrderedById = item.OrderedById,
                    OriginalRequestItemId = item.RequestItemId
                };
                // Додаємо запис у таблицю GeneralNeedsItems
                _context.GeneralNeedsItems.Add(gni);
            }

            await _context.SaveChangesAsync();

            // 4) Повідомляємо всіх WarehouseWorker 
            var whRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "WarehouseWorker");
            if (whRole != null)
            {
                var whUserIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == whRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                if (whUserIds.Any())
                {
                    foreach (var whId in whUserIds)
                    {
                        var notif = new Notification
                        {
                            RecipientId = whId,
                            Title = "Нова заявка у загальних потребах",
                            Message = $"Для заявки \"{request.Title}\" додано позиції в загальний список потреб.",
                            LinkUrl = "/warehouse-grouped",
                            Category = "Important",
                            RequestId = requestId
                        };
                        _context.Notifications.Add(notif);
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Перевіряє, чи Request (request.OrderedBy.DepartmentId) входить до переліку відділів, 
        /// де economistUserId є економістом (DepartmentEconomist).
        /// </summary>
        private async Task<bool> IsRequestInMyDepartments(Request request, string economistUserId)
        {
            if (request.OrderedById == null)
                return false;

            var author = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == request.OrderedById);
            if (author == null || author.DepartmentId == null)
                return false;

            var deptId = author.DepartmentId.Value;

            // Перевіряємо, чи цей deptId є серед DepartmentEconomist
            var isEconOfDept = await _context.DepartmentEconomists
                .AnyAsync(de => de.DepartmentId == deptId && de.EconomistId == economistUserId);

            return isEconOfDept;
        }

        public async Task<bool> IsEconomistOfDepartmentAsync(int departmentId, string economistUserId)
        {
            return await _context.DepartmentEconomists
                .AnyAsync(de => de.DepartmentId == departmentId && de.EconomistId == economistUserId);
        }
    }
}
