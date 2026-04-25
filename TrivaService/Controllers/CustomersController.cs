using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Infrastructure;
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

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var customers = await _unitOfWork.customerRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                customers = customers.Where(c =>
                    (c.CustomerName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (c.CustomerPhone?.ToLowerInvariant().Contains(term) ?? false) ||
                    (c.CustomerAddress?.ToLowerInvariant().Contains(term) ?? false));
            }

            return View(customers);
        }

        [HttpGet("/odata/customers")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var customers = await _unitOfWork.customerRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                customers = customers.Where(c =>
                    (c.CustomerName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (c.CustomerPhone?.ToLowerInvariant().Contains(term) ?? false) ||
                    (c.CustomerAddress?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(customers.OrderBy(c => c.CustomerName), skip, top);
            return Json(new { value = paged });
        }

        [HttpGet("/odata/customers/lookup")]
        public async Task<IActionResult> CustomerLookup([FromQuery] string? term, [FromQuery] int page = 1)
        {
            var customers = await _unitOfWork.customerRepository.GetAllAsync();
            var query = customers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim().ToLowerInvariant();
                query = query.Where(c => (c.CustomerName?.ToLowerInvariant().Contains(search) ?? false));
            }

            var result = query
                .OrderBy(c => c.CustomerName)
                .Skip((Math.Max(page, 1) - 1) * 20)
                .Take(20)
                .Select(c => ODataQueryHelpers.ToLookupResult(c.Id, c.CustomerName));

            return Json(new { value = result });
        }

        [HttpGet("/odata/customers/lookup/{id:int}")]
        public async Task<IActionResult> CustomerLookupById(int id)
        {
            var customer = await _unitOfWork.customerRepository.GetByIdAsync(id);
            if (customer is null)
            {
                return NotFound();
            }

            return Json(ODataQueryHelpers.ToLookupResult(customer.Id, customer.CustomerName));
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
