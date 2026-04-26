using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrivaService.Data;
using TrivaService.Services.Permissions;
using TrivaService.ViewModels.Auth;

namespace TrivaService.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IPermissionService _permissionService;

        public AuthController(AppDbContext dbContext, IPermissionService permissionService)
        {
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var user = await _dbContext.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.UserName == model.UserName && x.IsActive);

            if (user is null || user.UserPasswordHash != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya parola hatalı.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            await _permissionService.EnsureRolePermissionMatrixAsync(user.RolesId);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Roles.RoleName),
                new("UserId", user.Id.ToString()),
                new("RoleId", user.RolesId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
