using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
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

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var visuals = await _unitOfWork.serviceVisualsRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                visuals = visuals.Where(v =>
                    (v.ServiceVisualName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (v.Notes?.ToLowerInvariant().Contains(term) ?? false) ||
                    (v.ServiceDocumentUrl?.ToLowerInvariant().Contains(term) ?? false));
            }

            return View(visuals);
        }

        [HttpGet("/odata/servicevisuals")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var visuals = await _unitOfWork.serviceVisualsRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                visuals = visuals.Where(v =>
                    (v.ServiceVisualName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (v.Notes?.ToLowerInvariant().Contains(term) ?? false) ||
                    (v.ServiceDocumentUrl?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(visuals.OrderByDescending(v => v.Id), skip, top);
            return Json(new { value = paged });
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
