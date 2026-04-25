using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using TrivaService.Models.ServiceEntites;

namespace TrivaService.Controllers
{
    public class ServiceItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var serviceItems = await _unitOfWork.serviceItemRepository.GetAllAsync();

            var fNotes  = ODataQueryHelpers.ExtractFieldFilter(filter, "Notes");
            var fActive = ODataQueryHelpers.ExtractEqFilter(filter, "IsActive");

            if (!string.IsNullOrWhiteSpace(fNotes))
                serviceItems = serviceItems.Where(i => i.Notes?.ToLowerInvariant().Contains(fNotes.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fActive) && bool.TryParse(fActive, out var isActive))
                serviceItems = serviceItems.Where(i => i.IsActive == isActive);

            ViewBag.CurrentFilter = filter ?? string.Empty;
            return View(serviceItems);
        }

        [HttpGet("/odata/serviceitems")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var serviceItems = await _unitOfWork.serviceItemRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                serviceItems = serviceItems.Where(i => (i.Notes?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(serviceItems.OrderByDescending(i => i.Id), skip, top);
            return Json(new { value = paged });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var serviceItem = await _unitOfWork.serviceItemRepository.GetByIdAsync(id.Value);
            return serviceItem is null ? NotFound() : View(serviceItem);
        }

        public IActionResult Create()
        {
            return View(new ServiceItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceItem serviceItem)
        {
            if (!ModelState.IsValid)
                return View(serviceItem);

            var now = DateTime.UtcNow;
            serviceItem.Id = 0;
            serviceItem.CreateDate = now;
            serviceItem.UpdateDate = now;

            await _unitOfWork.serviceItemRepository.CreateAsync(serviceItem);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var serviceItem = await _unitOfWork.serviceItemRepository.GetByIdAsync(id.Value);
            return serviceItem is null ? NotFound() : View(serviceItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceItem serviceItem)
        {
            if (id != serviceItem.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(serviceItem);

            var existing = await _unitOfWork.serviceItemRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            existing.ServiceId = serviceItem.ServiceId;
            existing.ItemId = serviceItem.ItemId;
            existing.Quantity = serviceItem.Quantity;
            existing.UnitPrice = serviceItem.UnitPrice;
            existing.UnitCost = serviceItem.UnitCost;
            existing.TotalPrice = serviceItem.TotalPrice;
            existing.Notes = serviceItem.Notes;
            existing.IsActive = serviceItem.IsActive;
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.serviceItemRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var serviceItem = await _unitOfWork.serviceItemRepository.GetByIdAsync(id.Value);
            return serviceItem is null ? NotFound() : View(serviceItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.serviceItemRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
