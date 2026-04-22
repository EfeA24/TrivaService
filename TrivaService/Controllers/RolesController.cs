using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Models.UserEntities;

namespace TrivaService.Controllers
{
    public class RolesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public RolesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.rolesRepository.GetAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var role = await _unitOfWork.rolesRepository.GetByIdAsync(id.Value);
            return role is null ? NotFound() : View(role);
        }

        public IActionResult Create()
        {
            return View(new Roles());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Roles role)
        {
            if (!ModelState.IsValid)
                return View(role);

            var now = DateTime.UtcNow;
            role.Id = 0;
            role.CreateDate = now;
            role.UpdateDate = now;

            await _unitOfWork.rolesRepository.CreateAsync(role);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var role = await _unitOfWork.rolesRepository.GetByIdAsync(id.Value);
            return role is null ? NotFound() : View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Roles role)
        {
            if (id != role.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(role);

            var existing = await _unitOfWork.rolesRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            existing.RoleName = role.RoleName;
            existing.RoleDescription = role.RoleDescription;
            existing.IsActive = role.IsActive;
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.rolesRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var role = await _unitOfWork.rolesRepository.GetByIdAsync(id.Value);
            return role is null ? NotFound() : View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.rolesRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
