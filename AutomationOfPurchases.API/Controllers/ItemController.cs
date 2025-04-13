using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/item
        [HttpGet]
        public async Task<ActionResult<List<ItemDTO>>> GetItems()
        {
            // Вибираємо з таблиці Items
            var items = await _context.Items.ToListAsync();

            // Мапимо Item -> ItemDTO (поки що вручну)
            var result = items.Select(i => new ItemDTO
            {
                ItemId = i.ItemId,
                ItemName = i.ItemName,
                StorageUnit = i.StorageUnit
            }).ToList();

            return Ok(result);
        }
    }
}
