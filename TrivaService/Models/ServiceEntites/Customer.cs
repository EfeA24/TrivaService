using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.ServiceEntites
{
    public class Customer : BaseEntity
    {
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string? CustomerAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public int TotalServiceCount { get; set; }

        public List<Service> Services { get; set; } = new List<Service>();
    }
}
