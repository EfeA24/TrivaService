using TrivaService.Models.StockEntities;
using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.ServiceEntites
{
    public class ServiceItem : BaseEntity
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal? UnitPrice { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalPrice { get; set; }

        public string? Notes { get; set; }
    }
}