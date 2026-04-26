using System.Security.Claims;
using TrivaService.Models.TechnicalEntities;
using TrivaService.ViewModels.Permissions;

namespace TrivaService.Services.Permissions
{
    public interface IPermissionService
    {
        Task<List<RoleEntityPermissionInputViewModel>> BuildPermissionMatrixAsync(int? roleId);
        Task SaveRolePermissionsAsync(int roleId, List<RoleEntityPermissionInputViewModel> permissions);
        Task EnsureRolePermissionMatrixAsync(int roleId);
        Task<bool> HasEntityPermissionAsync(ClaimsPrincipal user, string entityName, PermissionOperation operation);
        Task<bool> CanReadPropertyAsync(ClaimsPrincipal user, string entityName, string propertyName);
        Task<bool> CanWritePropertyAsync(ClaimsPrincipal user, string entityName, string propertyName);
        Task<HashSet<string>> GetReadablePropertiesAsync(ClaimsPrincipal user, string entityName);
        Task<HashSet<string>> GetWritablePropertiesAsync(ClaimsPrincipal user, string entityName);
        Task ApplyWritePermissionsAsync<T>(ClaimsPrincipal user, string entityName, T source, T target) where T : BaseEntity;
    }
}
