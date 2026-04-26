using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.UserEntities
{
    public class RoleEntityPermission : BaseEntity
    {
        public int RoleId { get; set; }
        public Roles Role { get; set; } = null!;
        public string EntityName { get; set; } = null!;

        public bool CanRead { get; set; } = true;
        public bool CanCreate { get; set; } = true;
        public bool CanUpdate { get; set; } = true;
        public bool CanDelete { get; set; } = true;

        public List<RolePropertyPermission> PropertyPermissions { get; set; } = new List<RolePropertyPermission>();
    }
}
