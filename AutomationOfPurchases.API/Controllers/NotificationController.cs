using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutomationOfPurchases.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/notification/my
        [HttpGet("my")]
        public async Task<ActionResult<List<NotificationDTO>>> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found.");

            var notifications = await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var result = notifications.Select(n => new NotificationDTO
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                LinkUrl = n.LinkUrl,

                // **НОВЕ**: передаємо RequestId
                RequestId = n.RequestId
            }).ToList();

            return Ok(result);
        }

        // PUT: api/notification/{id}/mark-read
        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Користувача не знайдено.");

            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.RecipientId == userId);

            if (notif == null)
                return NotFound("Поки немає повідомлень.");

            notif.IsRead = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
