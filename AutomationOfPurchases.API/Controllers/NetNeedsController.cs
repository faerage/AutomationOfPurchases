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
    public class NetNeedsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NetNeedsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Повертає «розширену» структуру списків чистих потреб.
        /// Кожен NetNeedsList -> NetNeedsListExpandedDTO
        /// з агрегованими ItemId.
        /// </summary>
        [HttpGet("expanded")]
        public async Task<ActionResult<List<NetNeedsListExpandedDTO>>> GetAllNetNeedsExpanded()
        {
            // 1) Завантажуємо списки NetNeedsList
            var lists = await _context.NetNeedsLists
                .Include(l => l.Items).ThenInclude(nni => nni.Item)
                .Include(l => l.Items).ThenInclude(nni => nni.OrderedBy).ThenInclude(u => u.Department)
                .ToListAsync();

            var result = new List<NetNeedsListExpandedDTO>();

            foreach (var l in lists)
            {
                // 2) Групуємо Items (NetNeedsItem) за ItemId
                var aggregatedItems = l.Items
                    .GroupBy(x => x.ItemId)
                    .Select(g =>
                    {
                        var first = g.First();
                        int sumQty = g.Sum(x => x.Quantity);

                        // Детальна інформація
                        var details = g.Select(x => new NetDetailDTO
                        {
                            RequestItemId = x.OriginalRequestItemId ?? 0, // або x.NetNeedsItemId
                            OrderedByFullName = x.OrderedBy?.FullName ?? "",
                            DepartmentName = x.OrderedBy?.Department?.DepartmentName ?? "",
                            Quantity = x.Quantity
                        }).ToList();

                        return new NetAggregatedItemDTO
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

                // 3) Формуємо «розширений» список
                var dto = new NetNeedsListExpandedDTO
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
