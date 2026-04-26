using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.UserEntities
{
    public class RolePropertyPermission : BaseEntity
    {
        public int RoleEntityPermissionId { get; set; }
        public RoleEntityPermission RoleEntityPermission { get; set; } = null!;
        public string PropertyName { get; set; } = null!;
        public bool CanRead { get; set; } = true;
        public bool CanWrite { get; set; } = true;
    }
}
