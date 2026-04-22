using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Models.UserEntities;

namespace TrivaService.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.usersRepository.GetAllAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var user = await _unitOfWork.usersRepository.GetByIdAsync(id.Value);
            return user is null ? NotFound() : View(user);
        }

        public IActionResult Create()
        {
            return View(new Users());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users user)
        {
            if (!ModelState.IsValid)
                return View(user);

            var now = DateTime.UtcNow;
            user.Id = 0;
            user.CreateDate = now;
            user.UpdateDate = now;

            await _unitOfWork.usersRepository.CreateAsync(user);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var user = await _unitOfWork.usersRepository.GetByIdAsync(id.Value);
            return user is null ? NotFound() : View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,UserPhone,UserNotes,RolesId,IsActive")] Users user, string? newPassword)
        {
            if (id != user.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(user);

            var existing = await _unitOfWork.usersRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            existing.UserName = user.UserName;
            existing.UserPhone = user.UserPhone;
            existing.UserNotes = user.UserNotes;
            existing.RolesId = user.RolesId;
            existing.IsActive = user.IsActive;
            existing.UpdateDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(newPassword))
                existing.UserPasswordHash = newPassword;

            await _unitOfWork.usersRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var user = await _unitOfWork.usersRepository.GetByIdAsync(id.Value);
            return user is null ? NotFound() : View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.usersRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
