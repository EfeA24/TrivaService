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
        private readonly IWebHostEnvironment _environment;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".mp4", ".mov", ".avi", ".mkv", ".webm"
        };

        public ServiceVisualsController(IUnitOfWork unitOfWork, IPermissionService permissionService, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
            _environment = environment;
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
        public async Task<IActionResult> Create(ServiceVisuals serviceVisuals, List<IFormFile>? mediaFiles)
        {
            if (!ModelState.IsValid)
                return View(serviceVisuals);

            var uploadedPaths = await SaveMediaFilesAsync(mediaFiles);
            if (!ModelState.IsValid)
                return View(serviceVisuals);

            var now = DateTime.UtcNow;
            var newEntity = new ServiceVisuals
            {
                Id = 0,
                CreateDate = now,
                UpdateDate = now,
                IsActive = true,
                ServiceDocumentUrl = string.Join(";", uploadedPaths)
            };
            await _permissionService.ApplyWritePermissionsAsync(User, nameof(ServiceVisuals), serviceVisuals, newEntity);
            if (uploadedPaths.Count > 0)
                newEntity.ServiceDocumentUrl = string.Join(";", uploadedPaths);

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
        public async Task<IActionResult> Edit(int id, ServiceVisuals serviceVisuals, List<IFormFile>? mediaFiles)
        {
            if (id != serviceVisuals.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(serviceVisuals);

            var existing = await _unitOfWork.serviceVisualsRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            var uploadedPaths = await SaveMediaFilesAsync(mediaFiles);
            if (!ModelState.IsValid)
                return View(serviceVisuals);

            await _permissionService.ApplyWritePermissionsAsync(User, nameof(ServiceVisuals), serviceVisuals, existing);
            if (uploadedPaths.Count > 0)
                existing.ServiceDocumentUrl = string.Join(";", uploadedPaths);
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

        private async Task<List<string>> SaveMediaFilesAsync(IEnumerable<IFormFile>? files)
        {
            var result = new List<string>();
            if (files is null)
                return result;

            var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "service-visuals");
            Directory.CreateDirectory(uploadRoot);

            foreach (var file in files)
            {
                if (file is null || file.Length == 0)
                    continue;

                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, $"Desteklenmeyen dosya tipi: {file.FileName}");
                    continue;
                }

                var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var physicalPath = Path.Combine(uploadRoot, safeFileName);
                await using var stream = System.IO.File.Create(physicalPath);
                await file.CopyToAsync(stream);

                result.Add($"/uploads/service-visuals/{safeFileName}");
            }

            return result;
        }
    }
}
