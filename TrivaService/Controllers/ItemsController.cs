using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using TrivaService.Models.StockEntities;

namespace TrivaService.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
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
            item.Id = 0;
            item.CreateDate = now;
            item.UpdateDate = now;

            await _unitOfWork.itemRepository.CreateAsync(item);
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

            existing.SupplierId = item.SupplierId;
            existing.ItemName = item.ItemName;
            existing.ItemCode = item.ItemCode;
            existing.ItemBrand = item.ItemBrand;
            existing.ItemModel = item.ItemModel;
            existing.ItemBarcode = item.ItemBarcode;
            existing.ItemDescription = item.ItemDescription;
            existing.ItemType = item.ItemType;
            existing.ItemQuantity = item.ItemQuantity;
            existing.ItemPrice = item.ItemPrice;
            existing.IsActive = item.IsActive;
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
