using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.UserEntities
{
    public class Users : BaseEntity
    {
        public string UserName { get; set; } = null!;
        public string UserPasswordHash { get; set; } = null!;
        public string? UserPhone { get; set; }
        public string? UserNotes{ get; set; }

        public Roles Roles { get; set; } = null!;
        public int RolesId { get; set; }
    }
}
