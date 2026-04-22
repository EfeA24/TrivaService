using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Models.StockEntities;

namespace TrivaService.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SuppliersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.supplierRepository.GetAllAsync());
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
            supplier.Id = 0;
            supplier.CreateDate = now;
            supplier.UpdateDate = now;

            await _unitOfWork.supplierRepository.CreateAsync(supplier);
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

            existing.SupplierName = supplier.SupplierName;
            existing.SupplierPhone = supplier.SupplierPhone;
            existing.SupplierContactPerson = supplier.SupplierContactPerson;
            existing.SupplierEmail = supplier.SupplierEmail;
            existing.SupplierAddress = supplier.SupplierAddress;
            existing.IsActive = supplier.IsActive;
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
