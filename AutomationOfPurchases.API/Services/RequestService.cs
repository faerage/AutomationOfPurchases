using AutomationOfPurchases.API.Repositories;
using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Enums;
using AutomationOfPurchases.Shared.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Services
{
    /// <summary>
    /// Сервіс для CRUD-операцій із заявками від імені “автора”:
    /// Створити, змінити, отримати список, видалити чернетку тощо.
    /// </summary>
    public class RequestService : IRequestService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public RequestService(AppDbContext context, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _context = context;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Створює нову заявку. За замовчуванням статус “PendingDepartmentHead” (якщо не вказано інакше).
        /// Надсилає повідомлення керівнику відділу автора, якщо він не співпадає з автором.
        /// </summary>
        public async Task<RequestDTO> CreateRequestAsync(RequestDTO requestDto, string orderedByUserId)
        {
            // 1) Мапимо з DTO у сутність Request
            var requestEntity = _mapper.Map<Request>(requestDto);

            // 2) Встановлюємо поля
            requestEntity.OrderedById = orderedByUserId;
            requestEntity.CreationDate = DateTime.UtcNow;

            // Якщо не задано статус
            if (string.IsNullOrWhiteSpace(requestEntity.Status))
            {
                requestEntity.Status = "PendingDepartmentHead";
            }

            // 3) Заповнюємо у RequestItem поле OrderedById
            if (requestEntity.Items != null)
            {
                foreach (var ri in requestEntity.Items)
                {
                    ri.OrderedById = orderedByUserId;
                }
            }

            // 4) Зберігаємо заявку
            _context.Requests.Add(requestEntity);
            await _context.SaveChangesAsync();

            // 5) Надсилаємо повідомлення керівнику (якщо він існує і не збігається з автором)
            var author = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == orderedByUserId);

            var departmentHeadId = author?.Department?.HeadOfDepartmentId;
            if (!string.IsNullOrEmpty(departmentHeadId) && departmentHeadId != orderedByUserId)
            {
                var notification = new Notification
                {
                    RecipientId = departmentHeadId,
                    Title = "Підлеглий створив нову заявку",
                    Message = $"Користувач \"{author?.FullName}\" створив нову заявку \"{requestEntity.Title}\".",
                    LinkUrl = $"/request-details/{requestEntity.RequestId}",
                    Category = "Important",
                    RequestId = requestEntity.RequestId
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            // 6) Повертаємо DTO створеної заявки
            return _mapper.Map<RequestDTO>(requestEntity);
        }

        /// <summary>
        /// Оновлює існуючу заявку, яку створив currentUser (тільки якщо вона ще в статусі Draft чи PendingDepartmentHead?).
        /// </summary>
        public async Task<RequestDTO?> UpdateRequestAsync(int requestId, RequestDTO requestDto, string userId)
        {
            // 1) Шукаємо заявку, що належить userId (якщо потрібна така логіка)
            var requestEntity = await _context.Requests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.OrderedById == userId);

            if (requestEntity == null)
                return null; // Не знайдено, або не належить поточному користувачеві

            // 2) Оновлюємо основні поля заявки (назва, опис, статус)
            requestEntity.Title = requestDto.Title;
            requestEntity.Description = requestDto.Description;
            requestEntity.Status = requestDto.Status.ToString();

            // 3) Очищаємо старі Items і додаємо нові (якщо треба)
            requestEntity.Items.Clear();
            if (requestDto.Items != null)
            {
                foreach (var itemDto in requestDto.Items)
                {
                    var newItem = new RequestItem
                    {
                        ItemId = itemDto.ItemId,
                        Quantity = itemDto.Quantity,
                        Satisfied = itemDto.Satisfied,
                        OrderedById = userId
                    };
                    requestEntity.Items.Add(newItem);
                }
            }

            // 4) Зберігаємо
            await _context.SaveChangesAsync();
            return _mapper.Map<RequestDTO>(requestEntity);
        }

        /// <summary>
        /// Повертає список заявок, створених певним користувачем (userId).
        /// </summary>
        public async Task<List<RequestDTO>> GetRequestsByUserAsync(string userId)
        {
            var requestEntities = await _context.Requests
                .Where(r => r.OrderedById == userId)
                .ToListAsync();

            return requestEntities
                .Select(r => _mapper.Map<RequestDTO>(r))
                .ToList();
        }

        /// <summary>
        /// Повертає заявку з деталями за Id (для керівника/економіста).
        /// Якщо потрібні поля DeliveredQuantity/ToPurchaseQuantity, вони беруться із RequestItem.
        /// </summary>
        public async Task<RequestDTO?> GetRequestByIdAsync(int id)
        {
            var entity = await _context.Requests
                .Include(r => r.OrderedBy).ThenInclude(u => u.Department)
                .Include(r => r.Items).ThenInclude(ri => ri.Item)
                .Include(r => r.EconomistApprover)
                .Include(r => r.DepartmentHeadApprover)
                .Include(r => r.RejectedByUser)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (entity == null) return null;

            return _mapper.Map<RequestDTO>(entity);
        }

        /// <summary>
        /// Повертає заявку лише якщо вона належить userId (для автора).
        /// </summary>
        public async Task<RequestDTO?> GetRequestByIdForUserAsync(int requestId, string userId)
        {
            var entity = await _context.Requests
                .Include(r => r.OrderedBy).ThenInclude(u => u.Department)
                .Include(r => r.Items).ThenInclude(ri => ri.Item)
                .Include(r => r.EconomistApprover)
                .Include(r => r.DepartmentHeadApprover)
                .Include(r => r.RejectedByUser)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.OrderedById == userId);

            if (entity == null) return null;

            return _mapper.Map<RequestDTO>(entity);
        }

        /// <summary>
        /// Видаляє чернетку (Draft) заявки, якщо вона належить userId.
        /// </summary>
        public async Task<bool> DeleteDraftAsync(int requestId, string userId)
        {
            var requestEntity = await _context.Requests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r =>
                    r.RequestId == requestId &&
                    r.OrderedById == userId &&
                    r.Status == "Draft");

            if (requestEntity == null)
                return false; 

            _context.RequestItems.RemoveRange(requestEntity.Items);
            _context.Requests.Remove(requestEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Повертає заявку з її деталями за Id (для керівника/економіста).
        /// Заповнює DeliveredQuantity і ToPurchaseQuantity для кожного RequestItemDTO.
        /// </summary>
        public async Task<RequestDTO?> GetRequestByIdAsync(int requestId, string currentUserId)
        {
            var entity = await _context.Requests
                .Include(r => r.OrderedBy).ThenInclude(u => u.Department)
                .Include(r => r.Items).ThenInclude(ri => ri.Item)
                // Додатково, якщо треба, .Include(r => r.Items).ThenInclude(ri => ri.DeliveryRequests)
                .Include(r => r.EconomistApprover)
                .Include(r => r.DepartmentHeadApprover)
                .Include(r => r.RejectedByUser)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (entity == null) return null;

            var dto = _mapper.Map<RequestDTO>(entity);
            var user = await _context.Users.FindAsync(currentUserId);
            if (user != null)
            {
                // Можна перевірити, чи має цей user роль "Economist" 
                // (наприклад, дивимося в таблицю UserRoles)
                // 1) Спочатку дістаємо ідентифікатор ролі економіста
                string? econRoleId = await GetRoleIdByName("Economist");

                // 2) А тоді використовуємо вже готове econRoleId
                bool isEconomist = await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == currentUserId && ur.RoleId == econRoleId);

                if (isEconomist && entity.OrderedBy?.DepartmentId != null)
                {
                    var deptId = entity.OrderedBy.DepartmentId.Value;
                    // 2) Перевіряємо, чи є currentUserId у DepartmentEconomists 
                    //    для deptId:
                    bool isEconOfDept = await _context.DepartmentEconomists
                        .AnyAsync(de => de.DepartmentId == deptId &&
                                        de.EconomistId == currentUserId);

                    dto.CanApproveAsEconomist = isEconOfDept;
                }
                else
                {
                    dto.CanApproveAsEconomist = false;
                }
            }

            return dto;
        }


        private async Task<string?> GetRoleIdByName(string roleName)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);
            return role?.Id;
        }
    }
}
