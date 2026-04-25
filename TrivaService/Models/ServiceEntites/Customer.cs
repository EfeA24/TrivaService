using System.ComponentModel.DataAnnotations;
using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Models.ServiceEntites
{
    public class Customer : BaseEntity
    {
        [Display(Name = "Müşteri Adı")]
        public string CustomerName { get; set; } = null!;
        [Display(Name = "Telefon")]
        public string CustomerPhone { get; set; } = null!;
        [Display(Name = "Adres")]
        public string? CustomerAddress { get; set; }
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }
        [Display(Name = "Son Servis Tarihi")]
        public DateTime? LastServiceDate { get; set; }
        [Display(Name = "Toplam Servis Sayısı")]
        public int TotalServiceCount { get; set; }

        public List<Service> Services { get; set; } = new List<Service>();
    }
}
