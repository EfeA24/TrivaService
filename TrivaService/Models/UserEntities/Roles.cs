using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.UserEntities
{
    public class Roles : BaseEntity
    {
        public string RoleName { get; set; } = null!;
        public string? RoleDescription { get; set; }

        public List<Users> Users { get; set; } = new List<Users>();
    }
}
