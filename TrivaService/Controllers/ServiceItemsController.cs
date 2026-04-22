using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
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

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.serviceItemRepository.GetAllAsync());
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
