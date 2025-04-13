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
    [Authorize(Roles = "WarehouseWorker")]
    public class WarehouseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WarehouseController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Повертає список ТМЦ із активного GeneralNeedsList (NullificationDate == null),
        /// згрупований за ItemId, для відображення на фронті.
        /// </summary>
        [HttpGet("grouped-needs")]
        public async Task<ActionResult<List<GroupedNeedDTO>>> GetGroupedNeeds()
        {
            // 1) Шукаємо активний GeneralNeedsList
            var activeList = await _context.GeneralNeedsLists
                .Include(l => l.Items).ThenInclude(gni => gni.Item)
                .Include(l => l.Items).ThenInclude(gni => gni.OrderedBy).ThenInclude(u => u.Department)
                .Where(l => l.NullificationDate == null)
                .FirstOrDefaultAsync();

            if (activeList == null)
                return Ok(new List<GroupedNeedDTO>()); // або NotFound(...)

            // 2) Групуємо GeneralNeedsItem за ItemId, щоб показати сумарні потреби
            var grouped = activeList.Items
                .GroupBy(x => x.ItemId)
                .Select(g =>
                {
                    var first = g.First();
                    int totalQty = g.Sum(it => it.Quantity);

                    return new GroupedNeedDTO
                    {
                        ItemId = first.ItemId,
                        ItemName = first.Item?.ItemName,
                        StorageUnit = first.Item?.StorageUnit,
                        TotalQuantity = totalQty,
                        IsExpanded = false, // Для Blazor

                        Requests = g.Select(x => new DepartmentRequestDTO
                        {
                            // У DepartmentRequestDTO.RequestItemId
                            // тимчасово зберігаємо GeneralNeedsItemId,
                            // щоб на фронті знати, що саме передати в SatisfyNeedModel:
                            RequestItemId = x.GeneralNeedsItemId,
                            DepartmentName = x.OrderedBy?.Department?.DepartmentName ?? "",
                            OrderedByFullName = x.OrderedBy?.FullName ?? "",
                            Quantity = x.Quantity,
                            OrderedById = x.OrderedById
                        }).ToList()
                    };
                })
                .Where(gr => gr.TotalQuantity > 0)
                .ToList();

            return Ok(grouped);
        }

        /// <summary>
        /// Задовольняє (QuantityToSatisfy) або позначає «Відсутньо» (IsNotAvailable) для 
        /// конкретного рядка GeneralNeedsItem, змінює оригінальний RequestItem 
        /// та надсилає повідомлення.
        /// </summary>
        [HttpPost("satisfy")]
        public async Task<IActionResult> SatisfyNeed([FromBody] SatisfyNeedModel model)
        {
            // 1) Знаходимо рядок у таблиці GeneralNeedsItem
            var gItem = await _context.GeneralNeedsItems
                .Include(gi => gi.Item)
                .Include(gi => gi.OrderedBy).ThenInclude(u => u.Department)
                .Include(gi => gi.OriginalRequestItem).ThenInclude(ri => ri.Item)
                .FirstOrDefaultAsync(gi => gi.GeneralNeedsItemId == model.GeneralNeedsItemId);

            if (gItem == null)
                return NotFound("Таблицю загальних потреб не знайдено.");

            // 2) Оригінальний рядок заявки (RequestItem)
            var origItem = gItem.OriginalRequestItem;
            if (origItem == null)
                return BadRequest("No original RequestItem is linked to this GeneralNeedsItem.");

            // 3) Скільки залишилося в оригіналі, щоб видати:
            // leftover = origItem.Quantity - (origItem.DeliveredQuantity + origItem.ToPurchaseQuantity)
            int leftover = origItem.Quantity - (origItem.DeliveredQuantity + origItem.ToPurchaseQuantity);
            if (leftover <= 0)
            {
                // В оригіналі вже нічого не залишилось
                return Ok("Не залишилось ТМЦ для задоволення.");
            }

            // 4) Дані для повідомлень
            var itemName = origItem.Item?.ItemName ?? "[N/A]";
            var originalRequestId = origItem.RequestId;
            var authorId = origItem.OrderedById;

            // Якщо треба отримати більше даних про автора (його відділ, керівника):
            var authorUser = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == authorId);

            var deptHeadId = authorUser?.Department?.HeadOfDepartmentId;

            // Користувач-склад, який зараз виконує дію:
            var warehouseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(warehouseUserId))
                return Unauthorized("ID працівника складу не знайдено.");

            // =========================================================
            // CASE A: "Відсутня" (IsNotAvailable = true)
            // =========================================================
            if (model.IsNotAvailable)
            {
                // Увесь leftover → ToPurchaseQuantity
                origItem.ToPurchaseQuantity += leftover;

                // Зменшуємо або обнуляємо gItem.Quantity
                gItem.Quantity = 0;
                // За бажання: _context.GeneralNeedsItems.Remove(gItem) // якщо хочете видаляти цілком
                // але тоді краще викликати SaveChanges() наприкінці

                // Додаємо запис у NetNeedsList (активний)
                var activeNet = await _context.NetNeedsLists
                    .FirstOrDefaultAsync(nl => nl.NullificationDate == null);
                if (activeNet == null)
                {
                    activeNet = new NetNeedsList
                    {
                        CreationDate = DateTime.UtcNow
                    };
                    _context.NetNeedsLists.Add(activeNet);
                    await _context.SaveChangesAsync();
                }

                var netItem = new NetNeedsItem
                {
                    NetNeedsListId = activeNet.ListId,
                    ItemId = origItem.ItemId,
                    Quantity = leftover,
                    OrderedById = authorId,
                    OriginalRequestItemId = origItem.RequestItemId
                };
                _context.NetNeedsItems.Add(netItem);

                // Повідомлення автору: “ТМЦ відсутня”
                if (!string.IsNullOrEmpty(authorId))
                {
                    var notifAuthor = new Notification
                    {
                        RecipientId = authorId,
                        Title = "ТМЦ відсутня на складі",
                        Message = $"Ваш запит на товар \"{itemName}\" позначено як відсутній.",
                        LinkUrl = $"/request-details/{originalRequestId}",
                        Category = "Rejected",
                        RequestId = originalRequestId
                    };
                    _context.Notifications.Add(notifAuthor);

                    // Повідомлення керівнику (якщо інший користувач)
                    if (!string.IsNullOrEmpty(deptHeadId) && deptHeadId != authorId)
                    {
                        var notifHead = new Notification
                        {
                            RecipientId = deptHeadId,
                            Title = "ТМЦ відсутня для підлеглого",
                            Message = $"Запит підлеглого (\"{authorUser?.FullName}\") на \"{itemName}\" позначено як відсутній на складі.",
                            LinkUrl = $"/request-details/{originalRequestId}",
                            Category = "Rejected",
                            RequestId = originalRequestId
                        };
                        _context.Notifications.Add(notifHead);
                    }
                }

                // Повідомлення економістам, що додано в NetNeedsList
                if (authorUser?.DepartmentId != null)
                {
                    var deptId = authorUser.DepartmentId.Value;
                    var econIds = await _context.DepartmentEconomists
                        .Where(de => de.DepartmentId == deptId)
                        .Select(de => de.EconomistId)
                        .ToListAsync();
                    foreach (var econId in econIds)
                    {
                        var notifEcon = new Notification
                        {
                            RecipientId = econId,
                            Title = "Нове додавання в чисті потреби",
                            Message = $"Для товару \"{itemName}\" на складі відсутньо {leftover} шт, тому додано до таблиці чистих потреб.",
                            LinkUrl = "/net-needs",
                            Category = "Rejected",
                            RequestId = originalRequestId
                        };
                        _context.Notifications.Add(notifEcon);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok($"ТМЦ позначено як відсутнє на складах, {leftover} додано до таблиці чистих потреб.");
            }

            // =========================================================
            // CASE B: "Задовольнити" (IsNotAvailable = false)
            // =========================================================
            if (model.QuantityToSatisfy <= 0)
            {
                return BadRequest("Не вірно введена кількість.");
            }

            int canGiveNow = Math.Min(leftover, model.QuantityToSatisfy);

            // 1) Збільшуємо DeliveredQuantity в оригіналі
            origItem.DeliveredQuantity += canGiveNow;

            // 2) Зменшуємо кількість у GeneralNeedsItem
            if (canGiveNow <= gItem.Quantity)
            {
                gItem.Quantity -= canGiveNow;
            }
            else
            {
                // На випадок, якщо canGiveNow > gItem.Quantity
                gItem.Quantity = 0;
            }

            // 3) Створюємо DeliveryRequest
            var warehouseName = string.IsNullOrWhiteSpace(model.WarehouseName)
                ? "Основний склад"
                : model.WarehouseName;

            var delivery = new DeliveryRequest
            {
                RequestItemId = origItem.RequestItemId,
                ItemId = origItem.ItemId,
                Quantity = canGiveNow,
                OrderedById = warehouseUserId,
                Warehouse = warehouseName
            };
            _context.DeliveryRequests.Add(delivery);

            // 4) Повідомлення
            var isFullyThisItem = (canGiveNow == leftover);
            // тобто повністю видано, що залишалося
            var msgText = isFullyThisItem
                ? $"повністю задоволено ({canGiveNow} шт.)."
                : $"частково задоволено ({canGiveNow} шт. із {leftover}).";

            if (!string.IsNullOrEmpty(authorId))
            {
                var notifAuthor = new Notification
                {
                    RecipientId = authorId,
                    Title = "ТМЦ отримано зі складу",
                    Message = $"Ваш запит на товар \"{itemName}\" було {msgText}",
                    LinkUrl = $"/request-details/{originalRequestId}",
                    Category = "Approved",
                    RequestId = originalRequestId
                };
                _context.Notifications.Add(notifAuthor);

                if (!string.IsNullOrEmpty(deptHeadId) && deptHeadId != authorId)
                {
                    var notifHead = new Notification
                    {
                        RecipientId = deptHeadId,
                        Title = isFullyThisItem
                            ? "Запит підлеглого повністю задоволено"
                            : "Запит підлеглого частково задоволено",
                        Message = isFullyThisItem
                            ? $"Склад видав {canGiveNow} шт. товару \"{itemName}\" (повне задоволення)."
                            : $"Склад видав {canGiveNow} шт. товару \"{itemName}\". (Залишок {origItem.Quantity - (origItem.DeliveredQuantity + origItem.ToPurchaseQuantity)}).",
                        LinkUrl = $"/request-details/{originalRequestId}",
                        Category = "Approved",
                        RequestId = originalRequestId
                    };
                    _context.Notifications.Add(notifHead);
                }
            }

            await _context.SaveChangesAsync();

            // 5) Повертаємо повідомлення в залежності від того, чи повністю
            if (canGiveNow < model.QuantityToSatisfy)
            {
                // Користувач хотів видати більше, ніж було leftover => часткова видача
                return Ok($"Ви задовільнили {canGiveNow}, не задоволено ще {leftover - canGiveNow}.");
            }
            else if (!isFullyThisItem)
            {
                // leftover > canGiveNow => часткова
                int remain = leftover - canGiveNow;
                return Ok($"Ви задовільнили {canGiveNow}, не задоволено ще {remain}.");
            }
            else
            {
                // canGiveNow == leftover => залишок = 0 => fully satisfied
                return Ok("Потреба повністю задоволена.");
            }
        }

    }
}
