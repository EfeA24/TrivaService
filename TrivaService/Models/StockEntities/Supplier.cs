using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.StockEntities
{
    public class Supplier : BaseEntity
    {
        public string SupplierName { get; set; } = null!;
        public string? SupplierPhone{ get; set; }
        public string? SupplierContactPerson { get; set; }
        public string? SupplierEmail { get; set; }
        public string? SupplierAddress { get; set; }

        public List<Item> Items { get; set; } = new List<Item>();
    }
}
