using TrivaService.Models.ServiceEntites;
using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.StockEntities
{
    public class Item : BaseEntity
    {
        public string ItemName { get; set; } = null!;
        public string? ItemCode { get; set; } = string.Empty;
        public string? ItemBrand { get; set; } = string.Empty;
        public string? ItemModel { get; set; } = string.Empty;
        public string? ItemBarcode { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public string? ItemType { get; set; }
        public int ItemQuantity { get; set; }
        public decimal? ItemPrice { get; set; }

        public Supplier Supplier { get; set; } = null!;
        public int SupplierId { get; set; }
        public List<ServiceItem> ServiceItems { get; set; } = new();
    }
}