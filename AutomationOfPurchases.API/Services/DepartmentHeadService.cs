using AutomationOfPurchases.API.Repositories;
using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Services
{
    public class DepartmentHeadService : IDepartmentHeadService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public DepartmentHeadService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        /// <summary>
        /// Повертає всі заявки, які створили користувачі департаменту поточного керівника (крім статусу “Draft”).
        /// </summary>
        public async Task<List<RequestDTO>> GetRequestsInMyDepartmentAsync(string departmentHeadUserId)
        {
            // Знаходимо користувача-керівника
            var headUser = (await _unitOfWork.Users.FindAsync(u => u.Id == departmentHeadUserId)).FirstOrDefault();
            if (headUser == null) return new List<RequestDTO>();

            // Знаходимо департамент, де headOfDepartmentId == headUser.Id
            var dept = (await _unitOfWork.Departments.FindAsync(d => d.HeadOfDepartmentId == headUser.Id)).FirstOrDefault();
            if (dept == null) return new List<RequestDTO>();

            // Усі користувачі цього департаменту
            var usersInDept = await _unitOfWork.Users.FindAsync(u => u.DepartmentId == dept.DepartmentId);
            var usersInDeptIds = usersInDept.Select(u => u.Id).ToList();

            // Заявки від тих користувачів, виключаємо Draft
            var allRequests = await _unitOfWork.Requests.FindAsync(r =>
                r.OrderedById != null &&
                usersInDeptIds.Contains(r.OrderedById) &&
                r.Status != "Draft");

            // Мапимо Request -> RequestDTO
            return allRequests
                .Select(r => _mapper.Map<RequestDTO>(r))
                .ToList();
        }

        /// <summary>
        /// Затвердження заявки керівником відділу (статус → PendingEconomist) + повідомлення автору й економістам.
        /// </summary>
        public async Task<bool> ApproveRequestAsync(int requestId, string departmentHeadUserId)
        {
            // 1) Шукаємо заявку
            var request = await _unitOfWork.Requests.GetByIdAsync(requestId);
            if (request == null) return false;

            // 2) Перевіряємо, чи ця заявка належить моєму відділу
            if (!await IsRequestInMyDepartment(request, departmentHeadUserId)) return false;

            // 3) Змінюємо статус і зберігаємо
            request.Status = "PendingEconomist";
            request.ApprovedByDepartmentHead = true;
            request.DepartmentHeadApproverId = departmentHeadUserId;
            request.RejectedByUserId = null;
            request.RejectionReason = null;

            await _unitOfWork.Requests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // ---- 4) Створюємо повідомлення автору заявки, що її затвердив керівник ---
            if (!string.IsNullOrEmpty(request.OrderedById))
            {
                var notifToAuthor = new Notification
                {
                    RecipientId = request.OrderedById,
                    Title = "Керівник затвердив вашу заявку",
                    Message = $"Ваша заявка \"{request.Title}\" була затверджена керівником.",
                    LinkUrl = $"/request-details/{requestId}",
                    Category = "Approved",
                    RequestId = requestId// для позначення блідо-зеленим фоном, наприклад
                };
                _context.Notifications.Add(notifToAuthor);
            }

            // ---- 5) Повідомлення всім економістам того відділу, що приймають далі заявку ---
            // Знаходимо відділ автора заявки (OrderedById)
            if (!string.IsNullOrEmpty(request.OrderedById))
            {
                var author = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == request.OrderedById);

                var deptId = author?.DepartmentId;
                if (deptId.HasValue)
                {
                    // Шукаємо всіх економістів у цьому відділі (DepartmentEconomist)
                    var economistIds = await _context.DepartmentEconomists
                        .Where(de => de.DepartmentId == deptId.Value)
                        .Select(de => de.EconomistId)
                        .ToListAsync();

                    // Для кожного економіста - повідомлення
                    foreach (var econId in economistIds)
                    {
                        var notifToEcon = new Notification
                        {
                            RecipientId = econId,
                            Title = "Нова заявка для затвердження",
                            Message = $"У вашому відділі з’явилась заявка \"{request.Title}\", що потребує вашого затвердження.",
                            LinkUrl = $"/request-details/{requestId}",
                            Category = "Important", // “важливе” повідомлення
                             RequestId = requestId
                        };
                        _context.Notifications.Add(notifToEcon);
                    }
                }
            }

            // Зберігаємо всі створені повідомлення разом
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Відхилення заявки керівником відділу (статус → Rejected) + повідомлення автору.
        /// </summary>
        public async Task<bool> RejectRequestAsync(int requestId, string departmentHeadUserId, string reason)
        {
            // 1) Шукаємо заявку
            var request = await _unitOfWork.Requests.GetByIdAsync(requestId);
            if (request == null) return false;

            // 2) Перевірка, чи ця заявка з мого відділу
            if (!await IsRequestInMyDepartment(request, departmentHeadUserId)) return false;

            // 3) Статус → "Rejected"
            request.Status = "Rejected";
            request.ApprovedByDepartmentHead = false;
            request.RejectedByUserId = departmentHeadUserId;
            request.RejectionReason = reason;

            await _unitOfWork.Requests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // 4) Повідомлення автору
            if (!string.IsNullOrEmpty(request.OrderedById))
            {
                var notif = new Notification
                {
                    RecipientId = request.OrderedById,
                    Title = "Керівник відхилив вашу заявку",
                    Message = $"Ваша заявка \"{request.Title}\" була відхилена керівником. Причина: {reason}",
                    LinkUrl = $"/request-details/{requestId}",
                    Category = "Rejected",
                     RequestId = requestId
                };
                _context.Notifications.Add(notif);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        /// <summary>
        /// Перевіряє, чи заявка належить відділу, де departmentHeadUserId – керівник.
        /// </summary>
        private async Task<bool> IsRequestInMyDepartment(Request request, string departmentHeadUserId)
        {
            if (request.OrderedById == null) return false;

            // Знаходимо автора заявки
            var author = (await _unitOfWork.Users.FindAsync(u => u.Id == request.OrderedById)).FirstOrDefault();
            if (author == null) return false;

            var deptId = author.DepartmentId;
            if (deptId == null) return false;

            // Перевіряємо, чи цей dept має headOfDepartmentId = departmentHeadUserId
            var dept = await _unitOfWork.Departments
                .FindAsync(d => d.DepartmentId == deptId && d.HeadOfDepartmentId == departmentHeadUserId);

            // Якщо знайдено хоч один — отже, заявка належить поточному керівнику
            return dept.Any();
        }

        /// <summary>
        /// Опційний метод: список економістів департаменту
        /// </summary>
        public async Task<List<string>> GetEconomistsOfDepartment(AppDbContext context, int departmentId)
        {
            var econIds = await context.DepartmentEconomists
                .Where(de => de.DepartmentId == departmentId)
                .Select(de => de.EconomistId)
                .ToListAsync();
            return econIds;
        }
    }
}
