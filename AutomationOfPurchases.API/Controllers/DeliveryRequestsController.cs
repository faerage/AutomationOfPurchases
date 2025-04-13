using AutomationOfPurchases.Shared.DTOs;  // <-- Використовуємо DTO з Shared
using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "WarehouseWorker")]
    public class DeliveryRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeliveryRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/deliveryrequests
        [HttpGet]
        public async Task<ActionResult<List<DeliveryRequestDTO>>> GetAll()
        {
            // Завантажуємо DeliveryRequests разом із потрібними навігаційними властивостями
            var deliveries = await _context.DeliveryRequests
                .Include(d => d.Item)
                .Include(d => d.RequestItem).ThenInclude(ri => ri.OrderedBy).ThenInclude(u => u.Department)
                .ToListAsync();

            // Мапимо кожен DeliveryRequest -> DeliveryRequestDTO
            var result = deliveries.Select(d => new DeliveryRequestDTO
            {
                DeliveryRequestId = d.DeliveryRequestId,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                Warehouse = d.Warehouse,
                OrderedById = d.OrderedById,
                RequestItemId = d.RequestItemId,

                // Додаткові поля для відображення на клієнті:
                ItemName = d.Item?.ItemName ?? "",
                OrderedByFullName = d.RequestItem?.OrderedBy?.FullName ?? "",
                OrderedByDepartmentName = d.RequestItem?.OrderedBy?.Department?.DepartmentName ?? "",
                OrderedByEmail = d.RequestItem?.OrderedBy?.Email ?? ""
            }).ToList();

            return Ok(result);
        }
    }
}
