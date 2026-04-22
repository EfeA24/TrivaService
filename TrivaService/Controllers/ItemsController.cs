using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
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

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.itemRepository.GetAllAsync());
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
