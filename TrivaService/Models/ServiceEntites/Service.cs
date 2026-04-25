using System.ComponentModel.DataAnnotations;
using TrivaService.Models.StockEntities;
using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.ServiceEntites
{
    public class Service : BaseEntity
    {
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public string ServiceCode { get; set; } = null!;
        public string FaultDescription { get; set; } = null!;
        public string? ServiceDescription { get; set; }
        public string? ServiceNotes { get; set; }

        public string? ServiceAddress { get; set; }

        public DateTime ReceivedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public string Status { get; set; } = null!;

        public decimal? EstimatedCost { get; set; }
        public decimal? FinalCost { get; set; }

        [Display(Name = "Ödeme tamamlandı")]
        public bool IsPaymentComplete { get; set; }

        public List<ServiceItem> ServiceItems { get; set; } = new();
        public List<ServiceVisuals> ServiceVisuals { get; set; } = new();
    }
}
