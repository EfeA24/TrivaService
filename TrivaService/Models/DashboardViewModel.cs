namespace TrivaService.Models
{
    public class DashboardChartPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class DashboardViewModel
    {
        /// <summary>Tüm zamanlar — özet kartlar.</summary>
        public int CompletedServices { get; set; }
        public int IncompleteServices { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }

        public int TotalServices { get; set; }
        public int CancelledServices { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalItems { get; set; }
        public int TotalServiceItemLines { get; set; }

        /// <summary>Tüm zamanlar — özet kart.</summary>
        public int PaymentCompleteCount { get; set; }
        /// <summary>Tüm zamanlar — özet kart.</summary>
        public int PaymentIncompleteCount { get; set; }

        /// <summary>Ödeme grafiği için seçilen dönem.</summary>
        public int PaymentCompleteInRange { get; set; }
        public int PaymentIncompleteInRange { get; set; }

        public List<DashboardChartPoint> SupplierUsageInServices { get; set; } = new();
        public string RangeUsageKey { get; set; } = DashboardTimeRange.ThreeMonths;
        public string RangeUsageLabel { get; set; } = "3 ay";

        public List<DashboardChartPoint> SupplierStockQuantities { get; set; } = new();

        public List<DashboardChartPoint> ServicesPerCustomer { get; set; } = new();
        public string RangeCustomerKey { get; set; } = DashboardTimeRange.ThreeMonths;
        public string RangeCustomerLabel { get; set; } = "3 ay";

        public List<DashboardChartPoint> ServicesByStatus { get; set; } = new();
        public string RangeStatusKey { get; set; } = DashboardTimeRange.ThreeMonths;
        public string RangeStatusLabel { get; set; } = "3 ay";

        public string RangePaymentKey { get; set; } = DashboardTimeRange.ThreeMonths;
        public string RangePaymentLabel { get; set; } = "3 ay";

        public List<DashboardChartPoint> ServicesReceivedTimeline { get; set; } = new();
        public string RangeTimelineKey { get; set; } = DashboardTimeRange.ThreeMonths;
        public string RangeTimelineLabel { get; set; } = "3 ay";
        public string TimelineChartDescription { get; set; } = string.Empty;
    }
}
