using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TrivaService.Data;
using TrivaService.Models.TechnicalEntities;
using TrivaService.Models.UserEntities;
using TrivaService.ViewModels.Permissions;

namespace TrivaService.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private const string AdminRoleName = "Admin";
        private const string AdminUserName = "admin";

        private static readonly HashSet<string> IgnoredEntityNames = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(RoleEntityPermission),
            nameof(RolePropertyPermission)
        };

        private readonly AppDbContext _dbContext;

        public PermissionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RoleEntityPermissionInputViewModel>> BuildPermissionMatrixAsync(int? roleId)
        {
            var definitions = GetEntityDefinitions();
            var matrix = definitions.Select(x => new RoleEntityPermissionInputViewModel
            {
                EntityName = x.EntityName,
                Properties = x.Properties.Select(p => new RolePropertyPermissionInputViewModel
                {
                    PropertyName = p
                }).ToList()
            }).ToList();

            if (!roleId.HasValue)
            {
                return matrix;
            }

            var existing = await _dbContext.RoleEntityPermissions
                .Include(x => x.PropertyPermissions)
                .Where(x => x.RoleId == roleId.Value)
                .ToListAsync();

            foreach (var item in matrix)
            {
                var entityPermission = existing.FirstOrDefault(x => x.EntityName == item.EntityName);
                if (entityPermission is null)
                {
                    continue;
                }

                item.CanRead = entityPermission.CanRead;
                item.CanCreate = entityPermission.CanCreate;
                item.CanUpdate = entityPermission.CanUpdate;
                item.CanDelete = entityPermission.CanDelete;

                foreach (var prop in item.Properties)
                {
                    var existingProperty = entityPermission.PropertyPermissions.FirstOrDefault(x => x.PropertyName == prop.PropertyName);
                    if (existingProperty is null)
                    {
                        continue;
                    }

                    prop.CanRead = existingProperty.CanRead;
                    prop.CanWrite = existingProperty.CanWrite;
                }
            }

            return matrix;
        }

        public async Task SaveRolePermissionsAsync(int roleId, List<RoleEntityPermissionInputViewModel> permissions)
        {
            var definitions = GetEntityDefinitions();
            var allowedEntities = definitions.ToDictionary(x => x.EntityName, x => x.Properties, StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            var normalizedInput = permissions
                .Where(x => allowedEntities.ContainsKey(x.EntityName))
                .Select(x => new RoleEntityPermissionInputViewModel
                {
                    EntityName = x.EntityName,
                    CanRead = x.CanRead,
                    CanCreate = x.CanCreate,
                    CanUpdate = x.CanUpdate,
                    CanDelete = x.CanDelete,
                    Properties = x.Properties
                        .Where(p => allowedEntities[x.EntityName].Contains(p.PropertyName))
                        .Select(p => new RolePropertyPermissionInputViewModel
                        {
                            PropertyName = p.PropertyName,
                            CanRead = p.CanRead,
                            CanWrite = p.CanWrite && p.CanRead
                        }).ToList()
                })
                .ToList();

            var existing = await _dbContext.RoleEntityPermissions
                .Include(x => x.PropertyPermissions)
                .Where(x => x.RoleId == roleId)
                .ToListAsync();

            _dbContext.RoleEntityPermissions.RemoveRange(existing);
            await _dbContext.SaveChangesAsync();

            var entityPermissions = new List<RoleEntityPermission>();
            foreach (var item in normalizedInput)
            {
                var entityPermission = new RoleEntityPermission
                {
                    RoleId = roleId,
                    EntityName = item.EntityName,
                    CanRead = item.CanRead,
                    CanCreate = item.CanCreate,
                    CanUpdate = item.CanUpdate,
                    CanDelete = item.CanDelete,
                    CreateDate = now,
                    UpdateDate = now,
                    IsActive = true,
                    PropertyPermissions = item.Properties.Select(p => new RolePropertyPermission
                    {
                        PropertyName = p.PropertyName,
                        CanRead = p.CanRead,
                        CanWrite = p.CanWrite && p.CanRead,
                        CreateDate = now,
                        UpdateDate = now,
                        IsActive = true
                    }).ToList()
                };

                entityPermissions.Add(entityPermission);
            }

            await _dbContext.RoleEntityPermissions.AddRangeAsync(entityPermissions);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EnsureRolePermissionMatrixAsync(int roleId)
        {
            var hasAny = await _dbContext.RoleEntityPermissions.AnyAsync(x => x.RoleId == roleId);
            if (hasAny)
            {
                return;
            }

            var matrix = await BuildPermissionMatrixAsync(null);
            await SaveRolePermissionsAsync(roleId, matrix);
        }

        public async Task<bool> HasEntityPermissionAsync(ClaimsPrincipal user, string entityName, PermissionOperation operation)
        {
            if (IsAdminUser(user))
            {
                return true;
            }

            var roleId = GetRoleId(user);
            if (!roleId.HasValue)
            {
                return false;
            }

            await EnsureRolePermissionMatrixAsync(roleId.Value);

            var permission = await _dbContext.RoleEntityPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RoleId == roleId.Value && x.EntityName == entityName);

            if (permission is null)
            {
                return false;
            }

            return operation switch
            {
                PermissionOperation.Read => permission.CanRead,
                PermissionOperation.Create => permission.CanCreate,
                PermissionOperation.Update => permission.CanUpdate,
                PermissionOperation.Delete => permission.CanDelete,
                _ => false
            };
        }

        public async Task<bool> CanReadPropertyAsync(ClaimsPrincipal user, string entityName, string propertyName)
        {
            var properties = await GetReadablePropertiesAsync(user, entityName);
            return properties.Contains(propertyName);
        }

        public async Task<bool> CanWritePropertyAsync(ClaimsPrincipal user, string entityName, string propertyName)
        {
            var properties = await GetWritablePropertiesAsync(user, entityName);
            return properties.Contains(propertyName);
        }

        public async Task<HashSet<string>> GetReadablePropertiesAsync(ClaimsPrincipal user, string entityName)
        {
            if (IsAdminUser(user))
            {
                return GetAllEntityProperties(entityName);
            }

            var roleId = GetRoleId(user);
            if (!roleId.HasValue)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            await EnsureRolePermissionMatrixAsync(roleId.Value);

            var entityPermission = await _dbContext.RoleEntityPermissions
                .Include(x => x.PropertyPermissions)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RoleId == roleId.Value && x.EntityName == entityName);

            if (entityPermission is null || !entityPermission.CanRead)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return entityPermission.PropertyPermissions
                .Where(x => x.CanRead)
                .Select(x => x.PropertyName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<HashSet<string>> GetWritablePropertiesAsync(ClaimsPrincipal user, string entityName)
        {
            if (IsAdminUser(user))
            {
                return GetAllEntityProperties(entityName);
            }

            var roleId = GetRoleId(user);
            if (!roleId.HasValue)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            await EnsureRolePermissionMatrixAsync(roleId.Value);

            var entityPermission = await _dbContext.RoleEntityPermissions
                .Include(x => x.PropertyPermissions)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RoleId == roleId.Value && x.EntityName == entityName);

            if (entityPermission is null || (!entityPermission.CanUpdate && !entityPermission.CanCreate))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return entityPermission.PropertyPermissions
                .Where(x => x.CanWrite)
                .Select(x => x.PropertyName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task ApplyWritePermissionsAsync<T>(ClaimsPrincipal user, string entityName, T source, T target) where T : BaseEntity
        {
            var writableProperties = await GetWritablePropertiesAsync(user, entityName);
            if (writableProperties.Count == 0)
            {
                return;
            }

            foreach (var prop in GetWritableProperties(typeof(T)))
            {
                if (!writableProperties.Contains(prop.Name))
                {
                    continue;
                }

                var value = prop.GetValue(source);
                prop.SetValue(target, value);
            }
        }

        private static int? GetRoleId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("RoleId")?.Value;
            return int.TryParse(claim, out var value) ? value : null;
        }

        private static bool IsAdminUser(ClaimsPrincipal user)
        {
            var roleName = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(roleName, AdminRoleName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var userName = user.Identity?.Name;
            return string.Equals(userName, AdminUserName, StringComparison.OrdinalIgnoreCase);
        }

        private HashSet<string> GetAllEntityProperties(string entityName)
        {
            var entityType = _dbContext.Model.GetEntityTypes()
                .Select(x => x.ClrType)
                .FirstOrDefault(x => x is not null && string.Equals(x.Name, entityName, StringComparison.OrdinalIgnoreCase));

            if (entityType is null)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return GetWritableProperties(entityType)
                .Select(x => x.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private List<(string EntityName, List<string> Properties)> GetEntityDefinitions()
        {
            return _dbContext.Model.GetEntityTypes()
                .Select(x => x.ClrType)
                .Where(x => x is not null && typeof(BaseEntity).IsAssignableFrom(x))
                .Where(x => !IgnoredEntityNames.Contains(x.Name))
                .Select(x => (x.Name, GetWritableProperties(x).Select(p => p.Name).ToList()))
                .OrderBy(x => x.Name)
                .ToList();
        }

        private static IEnumerable<PropertyInfo> GetWritableProperties(Type clrType)
        {
            return clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.GetSetMethod() is not null)
                .Where(x => IsSimpleType(x.PropertyType));
        }

        private static bool IsSimpleType(Type type)
        {
            var actualType = Nullable.GetUnderlyingType(type) ?? type;
            if (actualType.IsEnum)
            {
                return true;
            }

            return actualType.IsPrimitive
                   || actualType == typeof(string)
                   || actualType == typeof(decimal)
                   || actualType == typeof(DateTime)
                   || actualType == typeof(DateTimeOffset)
                   || actualType == typeof(TimeSpan)
                   || actualType == typeof(Guid);
        }
    }
}
