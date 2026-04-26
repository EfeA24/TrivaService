using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using TrivaService.Services.Permissions;
using ServiceEntity = TrivaService.Models.ServiceEntites.Service;

namespace TrivaService.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;

        public ServicesController(IUnitOfWork unitOfWork, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var services = await _unitOfWork.serviceRepository.GetAllAsync();

            var fCode    = ODataQueryHelpers.ExtractFieldFilter(filter, "ServiceCode");
            var fStatus  = ODataQueryHelpers.ExtractFieldFilter(filter, "Status");
            var fFault   = ODataQueryHelpers.ExtractFieldFilter(filter, "FaultDescription");
            var fAddress = ODataQueryHelpers.ExtractFieldFilter(filter, "ServiceAddress");
            var fActive  = ODataQueryHelpers.ExtractEqFilter(filter, "IsActive");

            if (!string.IsNullOrWhiteSpace(fCode))
                services = services.Where(s => s.ServiceCode?.ToLowerInvariant().Contains(fCode.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fStatus))
                services = services.Where(s => s.Status?.ToLowerInvariant().Contains(fStatus.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fFault))
                services = services.Where(s => s.FaultDescription?.ToLowerInvariant().Contains(fFault.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fAddress))
                services = services.Where(s => s.ServiceAddress?.ToLowerInvariant().Contains(fAddress.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fActive) && bool.TryParse(fActive, out var isActive))
                services = services.Where(s => s.IsActive == isActive);

            ViewBag.CurrentFilter = filter ?? string.Empty;
            return View(services);
        }

        [HttpGet("/odata/services")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var services = await _unitOfWork.serviceRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                services = services.Where(s =>
                    (s.ServiceCode?.ToLowerInvariant().Contains(term) ?? false) ||
                    (s.Status?.ToLowerInvariant().Contains(term) ?? false) ||
                    (s.ServiceAddress?.ToLowerInvariant().Contains(term) ?? false) ||
                    (s.FaultDescription?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(services.OrderByDescending(s => s.Id), skip, top);
            return Json(new { value = paged });
        }

        [HttpGet("/odata/services/lookup")]
        public async Task<IActionResult> ServiceLookup([FromQuery] string? term, [FromQuery] int page = 1)
        {
            var services = await _unitOfWork.serviceRepository.GetAllAsync();
            var customers = (await _unitOfWork.customerRepository.GetAllAsync()).ToDictionary(c => c.Id, c => c.CustomerName);
            var query = services.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim().ToLowerInvariant();
                query = query.Where(s =>
                {
                    customers.TryGetValue(s.CustomerId ?? 0, out var customerName);
                    var display = $"{s.ServiceCode} - {customerName ?? "Müşteri yok"}";
                    return display.ToLowerInvariant().Contains(search);
                });
            }

            var result = query
                .OrderByDescending(s => s.Id)
                .Skip((Math.Max(page, 1) - 1) * 20)
                .Take(20)
                .Select(s =>
                {
                    customers.TryGetValue(s.CustomerId ?? 0, out var customerName);
                    return ODataQueryHelpers.ToLookupResult(s.Id, $"{s.ServiceCode} - {customerName ?? "Müşteri yok"}");
                });

            return Json(new { value = result });
        }

        [HttpGet("/odata/services/lookup/{id:int}")]
        public async Task<IActionResult> ServiceLookupById(int id)
        {
            var service = await _unitOfWork.serviceRepository.GetByIdAsync(id);
            if (service is null)
            {
                return NotFound();
            }

            var customerName = "Müşteri yok";
            if (service.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.customerRepository.GetByIdAsync(service.CustomerId.Value);
                if (customer is not null)
                {
                    customerName = customer.CustomerName;
                }
            }

            return Json(ODataQueryHelpers.ToLookupResult(service.Id, $"{service.ServiceCode} - {customerName}"));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var service = await _unitOfWork.serviceRepository.GetByIdAsync(id.Value);
            return service is null ? NotFound() : View(service);
        }

        public IActionResult Create()
        {
            return View(new ServiceEntity
            {
                ReceivedDate = DateTime.UtcNow,
                Status = "Received"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceEntity service)
        {
            if (!ModelState.IsValid)
                return View(service);

            var now = DateTime.UtcNow;
            var newEntity = new ServiceEntity
            {
                Id = 0,
                CreateDate = now,
                UpdateDate = now,
                IsActive = true
            };
            await _permissionService.ApplyWritePermissionsAsync(User, nameof(ServiceEntity), service, newEntity);
            if (newEntity.ReceivedDate == default)
                newEntity.ReceivedDate = now;
            if (string.IsNullOrWhiteSpace(newEntity.Status))
                newEntity.Status = "Received";

            await _unitOfWork.serviceRepository.CreateAsync(newEntity);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var service = await _unitOfWork.serviceRepository.GetByIdAsync(id.Value);
            return service is null ? NotFound() : View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceEntity service)
        {
            if (id != service.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(service);

            var existing = await _unitOfWork.serviceRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _permissionService.ApplyWritePermissionsAsync(User, nameof(ServiceEntity), service, existing);
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.serviceRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var service = await _unitOfWork.serviceRepository.GetByIdAsync(id.Value);
            return service is null ? NotFound() : View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.serviceRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
