using System.ComponentModel.DataAnnotations;

namespace TrivaService.Models.TechnicalEntities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
}
