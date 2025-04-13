using AutomationOfPurchases.Shared.DTOs;
using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Economist")]
    public class GeneralNeedsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GeneralNeedsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Повертає «розширену» структуру списків загальних потреб.
        /// Кожен GeneralNeedsList -> GeneralNeedsListExpandedDTO,
        /// а в ньому AggregatedItems, згруповані за ItemId.
        /// </summary>
        [HttpGet("expanded")]
        public async Task<ActionResult<List<GeneralNeedsListExpandedDTO>>> GetAllListsExpanded()
        {
            // 1) Завантажуємо всі (або тільки активні) GeneralNeedsList
            var lists = await _context.GeneralNeedsLists
                .Include(l => l.Items).ThenInclude(gni => gni.Item)
                .Include(l => l.Items).ThenInclude(gni => gni.OrderedBy).ThenInclude(u => u.Department)
                .ToListAsync();

            // 2) Формуємо колекцію DTO
            var result = new List<GeneralNeedsListExpandedDTO>();

            foreach (var l in lists)
            {
                // Групуємо Items за ItemId
                var aggregatedItems = l.Items
                    .GroupBy(x => x.ItemId)
                    .Select(g =>
                    {
                        // «Перший» елемент групи (щоб взяти ItemName тощо)
                        var first = g.First();

                        // Підсумовуємо загальну кількість
                        int sumQty = g.Sum(x => x.Quantity);

                        // Деталі: хто замовник, його відділ, скільки
                        var details = g.Select(x => new GeneralDetailDTO
                        {
                            RequestItemId = x.OriginalRequestItemId ?? 0, // або x.GeneralNeedsItemId
                            OrderedByFullName = x.OrderedBy?.FullName ?? "",
                            DepartmentName = x.OrderedBy?.Department?.DepartmentName ?? "",
                            Quantity = x.Quantity
                        }).ToList();

                        return new GeneralAggregatedItemDTO
                        {
                            ItemId = first.ItemId,
                            ItemName = first.Item?.ItemName ?? "",
                            StorageUnit = first.Item?.StorageUnit ?? "",
                            TotalQuantity = sumQty,
                            IsExpanded = false, // для Blazor
                            Details = details
                        };
                    })
                    .Where(ag => ag.TotalQuantity > 0)
                    .ToList();

                // Створюємо «розширений» DTO
                var dto = new GeneralNeedsListExpandedDTO
                {
                    ListId = l.ListId,
                    CreationDate = l.CreationDate,
                    NullificationDate = l.NullificationDate,
                    AggregatedItems = aggregatedItems
                };

                result.Add(dto);
            }

            return Ok(result);
        }
    }
}
