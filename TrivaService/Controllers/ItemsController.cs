using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using TrivaService.Models.StockEntities;
using TrivaService.Services.Permissions;

namespace TrivaService.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;

        public ItemsController(IUnitOfWork unitOfWork, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var items = await _unitOfWork.itemRepository.GetAllAsync();

            var fName   = ODataQueryHelpers.ExtractFieldFilter(filter, "ItemName");
            var fCode   = ODataQueryHelpers.ExtractFieldFilter(filter, "ItemCode");
            var fBrand  = ODataQueryHelpers.ExtractFieldFilter(filter, "ItemBrand");
            var fModel  = ODataQueryHelpers.ExtractFieldFilter(filter, "ItemModel");
            var fActive = ODataQueryHelpers.ExtractEqFilter(filter, "IsActive");

            if (!string.IsNullOrWhiteSpace(fName))
                items = items.Where(i => i.ItemName?.ToLowerInvariant().Contains(fName.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fCode))
                items = items.Where(i => i.ItemCode?.ToLowerInvariant().Contains(fCode.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fBrand))
                items = items.Where(i => i.ItemBrand?.ToLowerInvariant().Contains(fBrand.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fModel))
                items = items.Where(i => i.ItemModel?.ToLowerInvariant().Contains(fModel.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fActive) && bool.TryParse(fActive, out var isActive))
                items = items.Where(i => i.IsActive == isActive);

            ViewBag.CurrentFilter = filter ?? string.Empty;
            return View(items);
        }

        [HttpGet("/odata/items")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var items = await _unitOfWork.itemRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                items = items.Where(i =>
                    (i.ItemName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (i.ItemCode?.ToLowerInvariant().Contains(term) ?? false) ||
                    (i.ItemBrand?.ToLowerInvariant().Contains(term) ?? false) ||
                    (i.ItemModel?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(items.OrderBy(i => i.ItemName), skip, top);
            return Json(new { value = paged });
        }

        [HttpGet("/odata/items/lookup")]
        public async Task<IActionResult> ItemLookup([FromQuery] string? term, [FromQuery] int page = 1)
        {
            var items = await _unitOfWork.itemRepository.GetAllAsync();
            var query = items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim().ToLowerInvariant();
                query = query.Where(i =>
                    (i.ItemName?.ToLowerInvariant().Contains(search) ?? false) ||
                    (i.ItemCode?.ToLowerInvariant().Contains(search) ?? false));
            }

            var result = query
                .OrderBy(i => i.ItemName)
                .Skip((Math.Max(page, 1) - 1) * 20)
                .Take(20)
                .Select(i => ODataQueryHelpers.ToLookupResult(i.Id, string.IsNullOrWhiteSpace(i.ItemCode) ? i.ItemName : $"{i.ItemName} ({i.ItemCode})"));

            return Json(new { value = result });
        }

        [HttpGet("/odata/items/lookup/{id:int}")]
        public async Task<IActionResult> ItemLookupById(int id)
        {
            var item = await _unitOfWork.itemRepository.GetByIdAsync(id);
            if (item is null)
            {
                return NotFound();
            }

            var text = string.IsNullOrWhiteSpace(item.ItemCode) ? item.ItemName : $"{item.ItemName} ({item.ItemCode})";
            return Json(ODataQueryHelpers.ToLookupResult(item.Id, text));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var item = await _unitOfWork.itemRepository.GetByIdAsync(id.Value);
            return item is null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            return View(new Item());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item)
        {
            if (!ModelState.IsValid)
                return View(item);

            var now = DateTime.UtcNow;
            var newEntity = new Item
            {
                Id = 0,
                CreateDate = now,
                UpdateDate = now,
                IsActive = true
            };
            await _permissionService.ApplyWritePermissionsAsync(User, nameof(Item), item, newEntity);

            await _unitOfWork.itemRepository.CreateAsync(newEntity);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var item = await _unitOfWork.itemRepository.GetByIdAsync(id.Value);
            return item is null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(item);

            var existing = await _unitOfWork.itemRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _permissionService.ApplyWritePermissionsAsync(User, nameof(Item), item, existing);
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.itemRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var item = await _unitOfWork.itemRepository.GetByIdAsync(id.Value);
            return item is null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.itemRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
