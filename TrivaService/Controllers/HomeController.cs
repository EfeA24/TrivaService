using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrivaService.Data;
using TrivaService.Models;
using ServiceEntity = TrivaService.Models.ServiceEntites.Service;

namespace TrivaService.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            [FromQuery(Name = "ru")] string? rangeUsage,
            [FromQuery(Name = "rp")] string? rangePayment,
            [FromQuery(Name = "rc")] string? rangeCustomer,
            [FromQuery(Name = "rs")] string? rangeStatus,
            [FromQuery(Name = "rt")] string? rangeTimeline)
        {
            var ku = DashboardTimeRange.Normalize(rangeUsage);
            var kp = DashboardTimeRange.Normalize(rangePayment);
            var kc = DashboardTimeRange.Normalize(rangeCustomer);
            var ks = DashboardTimeRange.Normalize(rangeStatus);
            var kt = DashboardTimeRange.Normalize(rangeTimeline);

            var vm = await BuildDashboardAsync(ku, kp, kc, ks, kt);
            return View(vm);
        }

        private static IQueryable<ServiceEntity> ServicesInRange(
            IQueryable<ServiceEntity> source,
            DateTime? fromUtc,
            bool allTime)
        {
            if (!allTime && fromUtc.HasValue)
                return source.Where(s => s.ReceivedDate >= fromUtc.Value);
            return source;
        }

        private static string TimelineDescription(string rangeKey) =>
            rangeKey switch
            {
                DashboardTimeRange.OneDay => "Son 24 saat (saatlik)",
                DashboardTimeRange.OneWeek => "Son 7 gün (günlük)",
                DashboardTimeRange.OneMonth or DashboardTimeRange.ThreeMonths or DashboardTimeRange.SixMonths => "Günlük alınan servis sayısı",
                DashboardTimeRange.OneYear or DashboardTimeRange.FiveYears => "Aylık alınan servis sayısı",
                DashboardTimeRange.All => "Tüm kayıtlar (aylık)",
                _ => "Zaman çizelgesi"
            };

        private async Task<DashboardViewModel> BuildDashboardAsync(
            string rangeUsage,
            string rangePayment,
            string rangeCustomer,
            string rangeStatus,
            string rangeTimeline)
        {
            var utcNow = DateTime.UtcNow;

            var fromU = DashboardTimeRange.GetStartUtc(rangeUsage, utcNow, out var allU);
            var fromP = DashboardTimeRange.GetStartUtc(rangePayment, utcNow, out var allP);
            var fromC = DashboardTimeRange.GetStartUtc(rangeCustomer, utcNow, out var allC);
            var fromS = DashboardTimeRange.GetStartUtc(rangeStatus, utcNow, out var allS);
            var fromT = DashboardTimeRange.GetStartUtc(rangeTimeline, utcNow, out var allT);

            var vm = new DashboardViewModel
            {
                RangeUsageKey = rangeUsage,
                RangeUsageLabel = DashboardTimeRange.LabelFor(rangeUsage),
                RangePaymentKey = rangePayment,
                RangePaymentLabel = DashboardTimeRange.LabelFor(rangePayment),
                RangeCustomerKey = rangeCustomer,
                RangeCustomerLabel = DashboardTimeRange.LabelFor(rangeCustomer),
                RangeStatusKey = rangeStatus,
                RangeStatusLabel = DashboardTimeRange.LabelFor(rangeStatus),
                RangeTimelineKey = rangeTimeline,
                RangeTimelineLabel = DashboardTimeRange.LabelFor(rangeTimeline),
                TimelineChartDescription = TimelineDescription(rangeTimeline)
            };

            var allSvc = _db.Services.AsQueryable();

            vm.CompletedServices = await allSvc.CountAsync(s => s.Status == "Completed");
            vm.IncompleteServices = await allSvc.CountAsync(s =>
                s.Status != "Completed" && s.Status != "Cancelled");
            vm.CancelledServices = await allSvc.CountAsync(s => s.Status == "Cancelled");
            vm.TotalServices = await allSvc.CountAsync();

            vm.ActiveCustomers = await _db.Customers.CountAsync(c => c.IsActive);
            vm.InactiveCustomers = await _db.Customers.CountAsync(c => !c.IsActive);
            vm.TotalCustomers = await _db.Customers.CountAsync();

            vm.TotalItems = await _db.Items.CountAsync();
            vm.TotalServiceItemLines = await _db.ServiceItems.CountAsync();

            vm.PaymentCompleteCount = await allSvc.CountAsync(s => s.IsPaymentComplete);
            vm.PaymentIncompleteCount = await allSvc.CountAsync(s => !s.IsPaymentComplete);

            var svcUsage = ServicesInRange(allSvc, fromU, allU);
            vm.SupplierUsageInServices = await (
                from si in _db.ServiceItems
                join s in svcUsage on si.ServiceId equals s.Id
                join i in _db.Items on si.ItemId equals i.Id
                join sup in _db.Suppliers on i.SupplierId equals sup.Id
                group si by sup.SupplierName into g
                orderby g.Sum(x => x.Quantity) descending
                select new DashboardChartPoint
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.Quantity)
                }).Take(20).ToListAsync();

            vm.SupplierStockQuantities = await (
                from i in _db.Items
                join sup in _db.Suppliers on i.SupplierId equals sup.Id
                group i by sup.SupplierName into g
                orderby g.Sum(x => x.ItemQuantity) descending
                select new DashboardChartPoint
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.ItemQuantity)
                }).Take(20).ToListAsync();

            var svcPay = ServicesInRange(allSvc, fromP, allP);
            vm.PaymentCompleteInRange = await svcPay.CountAsync(s => s.IsPaymentComplete);
            vm.PaymentIncompleteInRange = await svcPay.CountAsync(s => !s.IsPaymentComplete);

            var svcCust = ServicesInRange(allSvc, fromC, allC);
            var perCustomer = await svcCust
                .Where(s => s.CustomerId != null)
                .GroupBy(s => s.CustomerId!.Value)
                .Select(g => new { CustomerId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(15)
                .ToListAsync();

            var topCustomerIds = perCustomer.Select(p => p.CustomerId).ToList();
            var customerNames = await _db.Customers
                .Where(c => topCustomerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CustomerName);

            vm.ServicesPerCustomer = perCustomer
                .Select(p => new DashboardChartPoint
                {
                    Label = customerNames.GetValueOrDefault(p.CustomerId, $"#{p.CustomerId}"),
                    Value = p.Count
                })
                .ToList();

            var unassigned = await svcCust.CountAsync(s => s.CustomerId == null);
            if (unassigned > 0)
            {
                vm.ServicesPerCustomer.Add(new DashboardChartPoint
                {
                    Label = "Müşteri atanmamış",
                    Value = unassigned
                });
            }

            var svcStatus = ServicesInRange(allSvc, fromS, allS);
            vm.ServicesByStatus = await svcStatus
                .GroupBy(s => s.Status)
                .Select(g => new DashboardChartPoint { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            vm.ServicesReceivedTimeline = await BuildTimelineAsync(rangeTimeline, fromT, allT, utcNow);

            return vm;
        }

        private async Task<List<DashboardChartPoint>> BuildTimelineAsync(
            string rangeKey,
            DateTime? fromUtc,
            bool allTime,
            DateTime utcNow)
        {
            var allSvc = _db.Services.AsQueryable();

            if (rangeKey == DashboardTimeRange.OneDay)
            {
                var windowStart = utcNow.AddDays(-1);
                var dates = await ServicesInRange(allSvc, windowStart, false)
                    .Select(s => s.ReceivedDate)
                    .ToListAsync();

                var points = new List<DashboardChartPoint>();
                for (var i = 0; i < 24; i++)
                {
                    var bStart = windowStart.AddHours(i);
                    var bEnd = bStart.AddHours(1);
                    var cnt = dates.Count(d => d >= bStart && d < bEnd);
                    points.Add(new DashboardChartPoint
                    {
                        Label = bStart.ToString("dd.MM HH") + "h",
                        Value = cnt
                    });
                }

                return points;
            }

            if (rangeKey == DashboardTimeRange.OneWeek)
            {
                var startDay = utcNow.Date.AddDays(-6);
                var rows = await ServicesInRange(allSvc, startDay, false)
                    .GroupBy(s => s.ReceivedDate.Date)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .ToListAsync();
                var dict = rows.ToDictionary(x => x.Day, x => x.Count);
                var points = new List<DashboardChartPoint>();
                for (var d = 0; d < 7; d++)
                {
                    var day = startDay.AddDays(d);
                    dict.TryGetValue(day, out var cnt);
                    points.Add(new DashboardChartPoint
                    {
                        Label = day.ToString("dd.MM"),
                        Value = cnt
                    });
                }

                return points;
            }

            if (rangeKey is DashboardTimeRange.OneMonth
                or DashboardTimeRange.ThreeMonths
                or DashboardTimeRange.SixMonths)
            {
                var svc = ServicesInRange(allSvc, fromUtc, allTime);
                var endDay = utcNow.Date;
                var rows = await svc
                    .GroupBy(s => s.ReceivedDate.Date)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .ToListAsync();
                var dict = rows.ToDictionary(x => x.Day, x => x.Count);
                var points = new List<DashboardChartPoint>();
                for (var day = fromUtc!.Value.Date; day <= endDay; day = day.AddDays(1))
                {
                    dict.TryGetValue(day, out var cnt);
                    points.Add(new DashboardChartPoint
                    {
                        Label = day.ToString("dd.MM.yy"),
                        Value = cnt
                    });
                }

                return points;
            }

            if (rangeKey is DashboardTimeRange.OneYear or DashboardTimeRange.FiveYears)
            {
                var svc = ServicesInRange(allSvc, fromUtc, allTime);
                var rows = await svc
                    .GroupBy(s => new { s.ReceivedDate.Year, s.ReceivedDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();
                var dict = rows.ToDictionary(x => (x.Year, x.Month), x => x.Count);
                var start = new DateTime(fromUtc!.Value.Year, fromUtc.Value.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var points = new List<DashboardChartPoint>();
                for (var cur = start; cur <= end; cur = cur.AddMonths(1))
                {
                    dict.TryGetValue((cur.Year, cur.Month), out var cnt);
                    points.Add(new DashboardChartPoint
                    {
                        Label = cur.ToString("yyyy-MM"),
                        Value = cnt
                    });
                }

                return points;
            }

            if (rangeKey == DashboardTimeRange.All)
            {
                var raw = await _db.Services
                    .GroupBy(s => new { s.ReceivedDate.Year, s.ReceivedDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();
                return raw.Select(x => new DashboardChartPoint
                {
                    Label = $"{x.Year:0000}-{x.Month:00}",
                    Value = x.Count
                }).ToList();
            }

            return new List<DashboardChartPoint>();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
