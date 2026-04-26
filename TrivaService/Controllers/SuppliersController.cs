using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using TrivaService.Models.StockEntities;
using TrivaService.Services.Permissions;

namespace TrivaService.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;

        public SuppliersController(IUnitOfWork unitOfWork, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var suppliers = await _unitOfWork.supplierRepository.GetAllAsync();

            var fName    = ODataQueryHelpers.ExtractFieldFilter(filter, "SupplierName");
            var fPhone   = ODataQueryHelpers.ExtractFieldFilter(filter, "SupplierPhone");
            var fContact = ODataQueryHelpers.ExtractFieldFilter(filter, "SupplierContactPerson");
            var fActive  = ODataQueryHelpers.ExtractEqFilter(filter, "IsActive");

            if (!string.IsNullOrWhiteSpace(fName))
                suppliers = suppliers.Where(s => s.SupplierName?.ToLowerInvariant().Contains(fName.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fPhone))
                suppliers = suppliers.Where(s => s.SupplierPhone?.ToLowerInvariant().Contains(fPhone.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fContact))
                suppliers = suppliers.Where(s => s.SupplierContactPerson?.ToLowerInvariant().Contains(fContact.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fActive) && bool.TryParse(fActive, out var isActive))
                suppliers = suppliers.Where(s => s.IsActive == isActive);

            ViewBag.CurrentFilter = filter ?? string.Empty;
            return View(suppliers);
        }

        [HttpGet("/odata/suppliers")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var suppliers = await _unitOfWork.supplierRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                suppliers = suppliers.Where(s =>
                    (s.SupplierName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (s.SupplierPhone?.ToLowerInvariant().Contains(term) ?? false) ||
                    (s.SupplierContactPerson?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(suppliers.OrderBy(s => s.SupplierName), skip, top);
            return Json(new { value = paged });
        }

        [HttpGet("/odata/suppliers/lookup")]
        public async Task<IActionResult> SupplierLookup([FromQuery] string? term, [FromQuery] int page = 1)
        {
            var suppliers = await _unitOfWork.supplierRepository.GetAllAsync();
            var query = suppliers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim().ToLowerInvariant();
                query = query.Where(s => (s.SupplierName?.ToLowerInvariant().Contains(search) ?? false));
            }

            var result = query
                .OrderBy(s => s.SupplierName)
                .Skip((Math.Max(page, 1) - 1) * 20)
                .Take(20)
                .Select(s => ODataQueryHelpers.ToLookupResult(s.Id, s.SupplierName));

            return Json(new { value = result });
        }

        [HttpGet("/odata/suppliers/lookup/{id:int}")]
        public async Task<IActionResult> SupplierLookupById(int id)
        {
            var supplier = await _unitOfWork.supplierRepository.GetByIdAsync(id);
            if (supplier is null)
            {
                return NotFound();
            }

            return Json(ODataQueryHelpers.ToLookupResult(supplier.Id, supplier.SupplierName));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var supplier = await _unitOfWork.supplierRepository.GetByIdAsync(id.Value);
            return supplier is null ? NotFound() : View(supplier);
        }

        public IActionResult Create()
        {
            return View(new Supplier());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid)
                return View(supplier);

            var now = DateTime.UtcNow;
            var newEntity = new Supplier
            {
                Id = 0,
                CreateDate = now,
                UpdateDate = now,
                IsActive = true
            };
            await _permissionService.ApplyWritePermissionsAsync(User, nameof(Supplier), supplier, newEntity);

            await _unitOfWork.supplierRepository.CreateAsync(newEntity);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var supplier = await _unitOfWork.supplierRepository.GetByIdAsync(id.Value);
            return supplier is null ? NotFound() : View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(supplier);

            var existing = await _unitOfWork.supplierRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _permissionService.ApplyWritePermissionsAsync(User, nameof(Supplier), supplier, existing);
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.supplierRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var supplier = await _unitOfWork.supplierRepository.GetByIdAsync(id.Value);
            return supplier is null ? NotFound() : View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.supplierRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
