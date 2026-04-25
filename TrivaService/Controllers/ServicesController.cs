using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
using ServiceEntity = TrivaService.Models.ServiceEntites.Service;

namespace TrivaService.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServicesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
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
            service.Id = 0;
            service.CreateDate = now;
            service.UpdateDate = now;
            if (service.ReceivedDate == default)
                service.ReceivedDate = now;
            if (string.IsNullOrWhiteSpace(service.Status))
                service.Status = "Received";

            await _unitOfWork.serviceRepository.CreateAsync(service);
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

            existing.CustomerId = service.CustomerId;
            existing.ServiceCode = service.ServiceCode;
            existing.FaultDescription = service.FaultDescription;
            existing.ServiceDescription = service.ServiceDescription;
            existing.ServiceNotes = service.ServiceNotes;
            existing.ServiceAddress = service.ServiceAddress;
            existing.ReceivedDate = service.ReceivedDate;
            existing.CompletedDate = service.CompletedDate;
            existing.DeliveredDate = service.DeliveredDate;
            existing.Status = service.Status;
            existing.EstimatedCost = service.EstimatedCost;
            existing.FinalCost = service.FinalCost;
            existing.IsActive = service.IsActive;
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
