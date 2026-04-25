namespace TrivaService.Models
{
    /// <summary>Sorgu parametresi: <c>range</c> ile eşleşen anahtarlar.</summary>
    public static class DashboardTimeRange
    {
        public const string OneDay = "1d";
        public const string OneWeek = "1w";
        public const string OneMonth = "1m";
        public const string ThreeMonths = "3m";
        public const string SixMonths = "6m";
        public const string OneYear = "1y";
        public const string FiveYears = "5y";
        public const string All = "all";

        public static readonly IReadOnlyList<(string Key, string Label)> Options = new[]
        {
            (OneDay, "1 gün"),
            (OneWeek, "1 hafta"),
            (OneMonth, "1 ay"),
            (ThreeMonths, "3 ay"),
            (SixMonths, "6 ay"),
            (OneYear, "1 yıl"),
            (FiveYears, "5 yıl"),
            (All, "Tüm zamanlar")
        };

        /// <summary>Geçersiz veya boş değerde varsayılan: 3 ay.</summary>
        public static string Normalize(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return ThreeMonths;
            var k = key.Trim().ToLowerInvariant();
            return Options.Any(o => o.Key == k) ? k : ThreeMonths;
        }

        public static string LabelFor(string normalizedKey) =>
            Options.FirstOrDefault(o => o.Key == normalizedKey).Label ?? "3 ay";

        /// <summary>Dönem başlangıcı (UTC). <paramref name="allTime"/> true ise null.</summary>
        public static DateTime? GetStartUtc(string normalizedKey, DateTime utcNow, out bool allTime)
        {
            allTime = normalizedKey == All;
            if (allTime)
                return null;

            return normalizedKey switch
            {
                OneDay => utcNow.AddDays(-1),
                OneWeek => utcNow.AddDays(-7),
                OneMonth => utcNow.AddMonths(-1),
                ThreeMonths => utcNow.AddMonths(-3),
                SixMonths => utcNow.AddMonths(-6),
                OneYear => utcNow.AddYears(-1),
                FiveYears => utcNow.AddYears(-5),
                _ => utcNow.AddMonths(-3)
            };
        }
    }
}
