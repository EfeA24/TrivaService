using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.ServiceEntites
{
    public class ServiceVisuals : BaseEntity
    {
        public string ServiceVisualName { get; set; } = null!;
        public string? ServiceDocumentUrl { get; set; }
        public string? Notes { get; set; }
    }
}
