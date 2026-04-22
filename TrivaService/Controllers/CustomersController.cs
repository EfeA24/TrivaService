using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Models.ServiceEntites;

namespace TrivaService.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.customerRepository.GetAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var customer = await _unitOfWork.customerRepository.GetByIdAsync(id.Value);
            return customer is null ? NotFound() : View(customer);
        }

        public IActionResult Create()
        {
            return View(new Customer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var now = DateTime.UtcNow;
            customer.Id = 0;
            customer.CreateDate = now;
            customer.UpdateDate = now;

            await _unitOfWork.customerRepository.CreateAsync(customer);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var customer = await _unitOfWork.customerRepository.GetByIdAsync(id.Value);
            return customer is null ? NotFound() : View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(customer);

            var existing = await _unitOfWork.customerRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            existing.CustomerName = customer.CustomerName;
            existing.CustomerPhone = customer.CustomerPhone;
            existing.CustomerAddress = customer.CustomerAddress;
            existing.Notes = customer.Notes;
            existing.LastServiceDate = customer.LastServiceDate;
            existing.TotalServiceCount = customer.TotalServiceCount;
            existing.IsActive = customer.IsActive;
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.customerRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var customer = await _unitOfWork.customerRepository.GetByIdAsync(id.Value);
            return customer is null ? NotFound() : View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.customerRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
