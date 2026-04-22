using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
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

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.serviceRepository.GetAllAsync());
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
