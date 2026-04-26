using Microsoft.AspNetCore.Mvc;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Data;
using TrivaService.Infrastructure;
using TrivaService.Models.UserEntities;
using TrivaService.Services.Permissions;
using TrivaService.ViewModels.Permissions;

namespace TrivaService.Controllers
{
    public class RolesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _dbContext;
        private readonly IPermissionService _permissionService;

        public RolesController(IUnitOfWork unitOfWork, AppDbContext dbContext, IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "$filter")] string? filter)
        {
            var roles = await _unitOfWork.rolesRepository.GetAllAsync();

            var fName   = ODataQueryHelpers.ExtractFieldFilter(filter, "RoleName");
            var fDesc   = ODataQueryHelpers.ExtractFieldFilter(filter, "RoleDescription");
            var fActive = ODataQueryHelpers.ExtractEqFilter(filter, "IsActive");

            if (!string.IsNullOrWhiteSpace(fName))
                roles = roles.Where(r => r.RoleName?.ToLowerInvariant().Contains(fName.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fDesc))
                roles = roles.Where(r => r.RoleDescription?.ToLowerInvariant().Contains(fDesc.ToLowerInvariant()) ?? false);
            if (!string.IsNullOrWhiteSpace(fActive) && bool.TryParse(fActive, out var isActive))
                roles = roles.Where(r => r.IsActive == isActive);

            ViewBag.CurrentFilter = filter ?? string.Empty;
            return View(roles);
        }

        [HttpGet("/odata/roles")]
        public async Task<IActionResult> ODataList(
            [FromQuery(Name = "$filter")] string? filter,
            [FromQuery(Name = "$top")] int? top,
            [FromQuery(Name = "$skip")] int? skip)
        {
            var roles = await _unitOfWork.rolesRepository.GetAllAsync();
            var term = ODataQueryHelpers.ExtractSearchTerm(filter).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(term))
            {
                roles = roles.Where(r =>
                    (r.RoleName?.ToLowerInvariant().Contains(term) ?? false) ||
                    (r.RoleDescription?.ToLowerInvariant().Contains(term) ?? false));
            }

            var paged = ODataQueryHelpers.ApplyPagination(roles.OrderBy(r => r.RoleName), skip, top);
            return Json(new { value = paged });
        }

        [HttpGet("/odata/roles/lookup")]
        public async Task<IActionResult> RoleLookup([FromQuery] string? term, [FromQuery] int page = 1)
        {
            var roles = await _unitOfWork.rolesRepository.GetAllAsync();
            var query = roles.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim().ToLowerInvariant();
                query = query.Where(r => (r.RoleName?.ToLowerInvariant().Contains(search) ?? false));
            }

            var result = query
                .OrderBy(r => r.RoleName)
                .Skip((Math.Max(page, 1) - 1) * 20)
                .Take(20)
                .Select(r => ODataQueryHelpers.ToLookupResult(r.Id, r.RoleName));

            return Json(new { value = result });
        }

        [HttpGet("/odata/roles/lookup/{id:int}")]
        public async Task<IActionResult> RoleLookupById(int id)
        {
            var role = await _unitOfWork.rolesRepository.GetByIdAsync(id);
            if (role is null)
            {
                return NotFound();
            }

            return Json(ODataQueryHelpers.ToLookupResult(role.Id, role.RoleName));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var role = await _unitOfWork.rolesRepository.GetByIdAsync(id.Value);
            return role is null ? NotFound() : View(role);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new RoleEditViewModel
            {
                Permissions = await _permissionService.BuildPermissionMatrixAsync(null)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Permissions = await _permissionService.BuildPermissionMatrixAsync(null);
                return View(model);
            }

            var now = DateTime.UtcNow;
            var role = new Roles
            {
                Id = 0,
                RoleName = model.RoleName,
                RoleDescription = model.RoleDescription,
                IsActive = model.IsActive,
                CreateDate = now,
                UpdateDate = now
            };

            await _unitOfWork.rolesRepository.CreateAsync(role);
            await _unitOfWork.SaveAsync();
            await _permissionService.SaveRolePermissionsAsync(role.Id, model.Permissions);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var role = await _unitOfWork.rolesRepository.GetByIdAsync(id.Value);
            if (role is null)
                return NotFound();

            var vm = new RoleEditViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
                IsActive = role.IsActive,
                Permissions = await _permissionService.BuildPermissionMatrixAsync(role.Id)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Permissions = await _permissionService.BuildPermissionMatrixAsync(model.Id);
                return View(model);
            }

            var existing = await _unitOfWork.rolesRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            existing.RoleName = model.RoleName;
            existing.RoleDescription = model.RoleDescription;
            existing.IsActive = model.IsActive;
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.rolesRepository.UpdateAsync(existing);
            await _unitOfWork.SaveAsync();
            await _permissionService.SaveRolePermissionsAsync(existing.Id, model.Permissions);
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
            var permissions = _dbContext.RoleEntityPermissions.Where(x => x.RoleId == id);
            _dbContext.RoleEntityPermissions.RemoveRange(permissions);
            await _dbContext.SaveChangesAsync();

            await _unitOfWork.rolesRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
