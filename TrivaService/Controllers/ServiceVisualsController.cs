using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using TrivaService.Models.ServiceEntites;
using TrivaService.Services.Permissions;

namespace TrivaService.Controllers
{
    public class ServiceVisualsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;

        public ServiceVisualsController(IUnitOfWork unitOfWork, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var visuals = await _unitOfWork.serviceVisualsRepository.GetAllAsync();

            var fVisualName = ODataQueryHelpers.ExtractFieldFilter(filter, "ServiceVisualName");
            var fNotes      = ODataQueryHelpers.ExtractFieldFilter(filter, "Notes");
            var fActive     = ODataQueryHelpers.ExtractEqFilter(filter, "IsActive");

            if (!string.IsNullOrWhiteSpace(fVisualName))
                visuals = visuals.Where(v => v.ServiceVisualName?.ToLowerInvariant().Contains(fVisualName.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fNotes))
                visuals = visuals.Where(v => v.Notes?.ToLowerInvariant().Contains(fNotes.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fActive) && bool.TryParse(fActive, out var isActive))
                visuals = visuals.Where(v => v.IsActive == isActive);

            ViewBag.CurrentFilter = filter ?? string.Empty;
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
            var newEntity = new ServiceVisuals
            {
                Id = 0,
                CreateDate = now,
                UpdateDate = now,
                IsActive = true
            };
            await _permissionService.ApplyWritePermissionsAsync(User, nameof(ServiceVisuals), serviceVisuals, newEntity);

            await _unitOfWork.serviceVisualsRepository.CreateAsync(newEntity);
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

            await _permissionService.ApplyWritePermissionsAsync(User, nameof(ServiceVisuals), serviceVisuals, existing);
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
