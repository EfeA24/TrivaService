using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Models.ServiceEntites;

namespace TrivaService.Controllers
{
    public class ServiceVisualsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceVisualsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.serviceVisualsRepository.GetAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var entity = await _unitOfWork.serviceVisualsRepository.GetByIdAsync(id.Value);
            return entity is null ? NotFound() : View(entity);
        }

        public IActionResult Create()
        {
            return View(new ServiceVisuals());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceVisuals serviceVisuals)
        {
            if (!ModelState.IsValid)
                return View(serviceVisuals);

            var now = DateTime.UtcNow;
            serviceVisuals.Id = 0;
            serviceVisuals.CreateDate = now;
            serviceVisuals.UpdateDate = now;

            await _unitOfWork.serviceVisualsRepository.CreateAsync(serviceVisuals);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var entity = await _unitOfWork.serviceVisualsRepository.GetByIdAsync(id.Value);
            return entity is null ? NotFound() : View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceVisuals serviceVisuals)
        {
            if (id != serviceVisuals.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(serviceVisuals);

            var existing = await _unitOfWork.serviceVisualsRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            existing.ServiceId = serviceVisuals.ServiceId;
            existing.ServiceVisualName = serviceVisuals.ServiceVisualName;
            existing.ServiceDocumentUrl = serviceVisuals.ServiceDocumentUrl;
            existing.Notes = serviceVisuals.Notes;
            existing.IsActive = serviceVisuals.IsActive;
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.serviceVisualsRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var entity = await _unitOfWork.serviceVisualsRepository.GetByIdAsync(id.Value);
            return entity is null ? NotFound() : View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.serviceVisualsRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
