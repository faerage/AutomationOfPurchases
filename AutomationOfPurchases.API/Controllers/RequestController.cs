using AutomationOfPurchases.API.Services;
using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IDepartmentHeadService _deptHeadService;
        private readonly IEconomistService _economistService;
        private readonly AppDbContext _context;

        public RequestController(
            IRequestService requestService,
            IDepartmentHeadService deptHeadService,
            IEconomistService economistService,
            AppDbContext context)
        {
            _requestService = requestService;
            _deptHeadService = deptHeadService;
            _economistService = economistService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] RequestDTO requestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var createdRequest = await _requestService.CreateRequestAsync(requestDto, userId);
            return Ok(createdRequest);
        }

        [HttpPut("{requestId}")]
        public async Task<IActionResult> UpdateRequest(int requestId, [FromBody] RequestDTO requestDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var updated = await _requestService.UpdateRequestAsync(requestId, requestDto, userId);
            if (updated == null)
                return NotFound("Заявка не знайдена або не належить поточному користувачу.");

            return Ok(updated);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var requests = await _requestService.GetRequestsByUserAsync(userId);
            return Ok(requests);
        }

        [HttpGet("my/{requestId}")]
        public async Task<IActionResult> GetMyRequestById(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var oneRequest = await _requestService.GetRequestByIdForUserAsync(requestId, userId);
            if (oneRequest == null)
                return NotFound("Заявка не знайдена або не належить поточному користувачу.");

            return Ok(oneRequest);
        }

        [HttpGet("department")]
        [Authorize(Roles = "DepartmentHead")]
        public async Task<IActionResult> GetDepartmentRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var requests = await _deptHeadService.GetRequestsInMyDepartmentAsync(userId);
            return Ok(requests);
        }

        [HttpPut("{requestId}/approve")]
        [Authorize(Roles = "DepartmentHead")]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var success = await _deptHeadService.ApproveRequestAsync(requestId, userId);
            if (!success)
                return NotFound("Заявку не знайдено, або у вас не має доступу.");

            return Ok("Заявку успішно затверджено.");
        }

        [HttpPut("{requestId}/reject")]
        [Authorize(Roles = "DepartmentHead")]
        public async Task<IActionResult> RejectRequest(int requestId, [FromBody] RejectModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var success = await _deptHeadService.RejectRequestAsync(requestId, userId, model.Reason);
            if (!success)
                return NotFound("Заявку не знайдено, або у вас не має доступу.");

            return Ok("Заявку успішно відхилено.");
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "DepartmentHead,Economist")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dto = await _requestService.GetRequestByIdAsync(id, userId);
            if (dto == null)
                return NotFound("Заявку не знайдено.");
            return Ok(dto);
        }

        [HttpDelete("draft/{requestId}")]
        public async Task<IActionResult> DeleteDraft(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var success = await _requestService.DeleteDraftAsync(requestId, userId);
            if (!success)
                return NotFound("Чернетка не знайдена або вона не належить поточному користувачу.");

            return Ok("Чернетку видалено успішно.");
        }

        [HttpGet("economist")]
        [Authorize(Roles = "Economist")]
        public async Task<IActionResult> GetEconomistRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var requests = await _economistService.GetRequestsInMyDepartmentAsync(userId);
            return Ok(requests);
        }

        [HttpPut("{requestId}/approve-economist")]
        [Authorize(Roles = "Economist")]
        public async Task<IActionResult> ApproveEconomistRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var success = await _economistService.ApproveRequestAsync(requestId, userId);
            if (!success)
                return NotFound("Заявку не знайдено, або у вас не має доступу.");

            return Ok("Заявку успішно затверджено.");
        }

        [HttpPut("{requestId}/reject-economist")]
        [Authorize(Roles = "Economist")]
        public async Task<IActionResult> RejectEconomistRequest(int requestId, [FromBody] RejectModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var success = await _economistService.RejectRequestAsync(requestId, userId, model.Reason);
            if (!success)
                return NotFound("Заявку не знайдено, або у вас не має доступу.");

            return Ok("Заявку успішно відхилено.");
        }

        // Новий ендпоінт для отримання заявок для фільтрації повідомлень.
        // Він повертає список RequestShortDTO для всіх заявок, по яким надходили повідомлення для поточного користувача.
        [HttpGet("notification-requests")]
        public async Task<IActionResult> GetNotificationRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            // Отримуємо всі унікальні RequestId із повідомлень поточного користувача
            var notifRequestIds = await _context.Notifications
                .Where(n => n.RecipientId == userId && n.RequestId.HasValue)
                .Select(n => n.RequestId.Value)
                .Distinct()
                .ToListAsync();

            if (!notifRequestIds.Any())
                return Ok(new List<RequestShortDTO>());

            var requests = await _context.Requests
                .Where(r => notifRequestIds.Contains(r.RequestId))
                .Select(r => new RequestShortDTO
                {
                    RequestId = r.RequestId,
                    Title = r.Title
                })
                .ToListAsync();

            return Ok(requests);
        }
    }
}
