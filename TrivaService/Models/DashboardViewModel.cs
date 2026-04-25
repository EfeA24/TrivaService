namespace TrivaService.Models
{
    public class DashboardChartPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class DashboardViewModel
    {
        public int CompletedServices { get; set; }
        public int IncompleteServices { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }

        public int TotalServices { get; set; }
        public int CancelledServices { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalItems { get; set; }
        public int TotalServiceItemLines { get; set; }

        public int PaymentCompleteCount { get; set; }
        public int PaymentIncompleteCount { get; set; }

        /// <summary>Servis kalemlerinde tedarikçiye göre kullanılan parça adedi.</summary>
        public List<DashboardChartPoint> SupplierUsageInServices { get; set; } = new();

        /// <summary>Tedarikçiye göre stokta bulunan ürün miktarı toplamı.</summary>
        public List<DashboardChartPoint> SupplierStockQuantities { get; set; } = new();

        /// <summary>Müşteri başına servis kayıt sayısı.</summary>
        public List<DashboardChartPoint> ServicesPerCustomer { get; set; } = new();

        public List<DashboardChartPoint> ServicesByStatus { get; set; } = new();

        /// <summary>Zaman çizelgesi (ReceivedDate); aralığa göre saat/gün/ay gruplu.</summary>
        public List<DashboardChartPoint> ServicesReceivedTimeline { get; set; } = new();

        /// <summary>Grafikler ve servisle ilgili özetler için seçilen aralık (örn. <c>3m</c>).</summary>
        public string SelectedRangeKey { get; set; } = DashboardTimeRange.ThreeMonths;

        /// <summary>Seçilen aralığın Türkçe etiketi.</summary>
        public string SelectedRangeLabel { get; set; } = "3 ay";

        /// <summary>Zaman çizelgesi kartı başlığı altı açıklama.</summary>
        public string TimelineChartDescription { get; set; } = string.Empty;
    }
}
