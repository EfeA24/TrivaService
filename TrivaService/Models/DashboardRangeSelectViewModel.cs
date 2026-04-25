namespace TrivaService.Models
{
    /// <summary>Ana sayfa grafik kartındaki dönem seçici için.</summary>
    public class DashboardRangeSelectViewModel
    {
        /// <summary>Query parametre adı (örn. <c>ru</c>).</summary>
        public string ParamName { get; set; } = string.Empty;

        public string SelectedKey { get; set; } = DashboardTimeRange.ThreeMonths;
    }
}
