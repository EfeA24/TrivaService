using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.ServiceEntites
{
    public class ServiceVisuals : BaseEntity
    {
        public int? ServiceId { get; set; }

        public string ServiceVisualName { get; set; } = null!;
        public string? ServiceDocumentUrl { get; set; }
        public string? Notes { get; set; }
    }
}
