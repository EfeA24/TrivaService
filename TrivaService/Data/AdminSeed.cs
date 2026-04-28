using Microsoft.EntityFrameworkCore;
using TrivaService.Models.UserEntities;
using TrivaService.Services.Permissions;

namespace TrivaService.Data
{
    public static class AdminSeed
    {
        private const string AdminRoleName = "Admin";
        private const string AdminUserName = "admin";
        private const string AdminPassword = "12345admin";

        public static async Task EnsureAdminUserWithFullPermissionsAsync(AppDbContext dbContext, IPermissionService permissionService)
        {
            var now = DateTime.UtcNow;

            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.RoleName == AdminRoleName);
            if (adminRole is null)
            {
                adminRole = new Roles
                {
                    RoleName = AdminRoleName,
                    RoleDescription = "Sistem yöneticisi (tam yetki).",
                    IsActive = true,
                    CreateDate = now,
                    UpdateDate = now
                };

                await dbContext.Roles.AddAsync(adminRole);
                await dbContext.SaveChangesAsync();
            }
            else if (!adminRole.IsActive)
            {
                adminRole.IsActive = true;
                adminRole.UpdateDate = now;
                await dbContext.SaveChangesAsync();
            }

            var permissions = await permissionService.BuildPermissionMatrixAsync(null);
            foreach (var entityPermission in permissions)
            {
                entityPermission.CanRead = true;
                entityPermission.CanCreate = true;
                entityPermission.CanUpdate = true;
                entityPermission.CanDelete = true;

                foreach (var propertyPermission in entityPermission.Properties)
                {
                    propertyPermission.CanRead = true;
                    propertyPermission.CanWrite = true;
                }
            }

            await permissionService.SaveRolePermissionsAsync(adminRole.Id, permissions);

            var adminUser = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == AdminUserName);
            if (adminUser is null)
            {
                adminUser = new Users
                {
                    UserName = AdminUserName,
                    UserPasswordHash = AdminPassword,
                    RolesId = adminRole.Id,
                    UserPhone = null,
                    UserNotes = "Varsayilan admin kullanicisi.",
                    IsActive = true,
                    CreateDate = now,
                    UpdateDate = now
                };

                await dbContext.Users.AddAsync(adminUser);
            }
            else
            {
                adminUser.UserPasswordHash = AdminPassword;
                adminUser.RolesId = adminRole.Id;
                adminUser.IsActive = true;
                adminUser.UpdateDate = now;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
