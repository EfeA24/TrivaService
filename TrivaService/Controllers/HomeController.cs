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

        public async Task<IActionResult> Index([FromQuery(Name = "range")] string? range)
        {
            var key = DashboardTimeRange.Normalize(range);
            var vm = await BuildDashboardAsync(key);
            return View(vm);
        }

        private IQueryable<ServiceEntity> ServicesInRange(DateTime? fromUtc, bool allTime)
        {
            var q = _db.Services.AsQueryable();
            if (!allTime && fromUtc.HasValue)
                q = q.Where(s => s.ReceivedDate >= fromUtc.Value);
            return q;
        }

        private async Task<DashboardViewModel> BuildDashboardAsync(string rangeKey)
        {
            var utcNow = DateTime.UtcNow;
            var fromUtc = DashboardTimeRange.GetStartUtc(rangeKey, utcNow, out var allTime);

            var vm = new DashboardViewModel
            {
                SelectedRangeKey = rangeKey,
                SelectedRangeLabel = DashboardTimeRange.LabelFor(rangeKey)
            };

            var svc = ServicesInRange(fromUtc, allTime);

            vm.CompletedServices = await svc.CountAsync(s => s.Status == "Completed");
            vm.IncompleteServices = await svc.CountAsync(s =>
                s.Status != "Completed" && s.Status != "Cancelled");
            vm.CancelledServices = await svc.CountAsync(s => s.Status == "Cancelled");
            vm.TotalServices = await svc.CountAsync();

            vm.ActiveCustomers = await _db.Customers.CountAsync(c => c.IsActive);
            vm.InactiveCustomers = await _db.Customers.CountAsync(c => !c.IsActive);
            vm.TotalCustomers = await _db.Customers.CountAsync();

            vm.TotalItems = await _db.Items.CountAsync();

            vm.TotalServiceItemLines = await (
                from si in _db.ServiceItems
                join s in _db.Services on si.ServiceId equals s.Id
                where allTime || (fromUtc.HasValue && s.ReceivedDate >= fromUtc.Value)
                select si).CountAsync();

            vm.PaymentCompleteCount = await svc.CountAsync(s => s.IsPaymentComplete);
            vm.PaymentIncompleteCount = await svc.CountAsync(s => !s.IsPaymentComplete);

            vm.SupplierUsageInServices = await (
                from si in _db.ServiceItems
                join s in _db.Services on si.ServiceId equals s.Id
                join i in _db.Items on si.ItemId equals i.Id
                join sup in _db.Suppliers on i.SupplierId equals sup.Id
                where allTime || (fromUtc.HasValue && s.ReceivedDate >= fromUtc.Value)
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

            var perCustomer = await svc
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

            var unassigned = await svc.CountAsync(s => s.CustomerId == null);
            if (unassigned > 0)
            {
                vm.ServicesPerCustomer.Add(new DashboardChartPoint
                {
                    Label = "Müşteri atanmamış",
                    Value = unassigned
                });
            }

            vm.ServicesByStatus = await svc
                .GroupBy(s => s.Status)
                .Select(g => new DashboardChartPoint { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            vm.ServicesReceivedTimeline = await BuildTimelineAsync(rangeKey, fromUtc, allTime, utcNow);
            vm.TimelineChartDescription = rangeKey switch
            {
                DashboardTimeRange.OneDay => "Son 24 saat (saatlik)",
                DashboardTimeRange.OneWeek => "Son 7 gün (günlük)",
                DashboardTimeRange.OneMonth or DashboardTimeRange.ThreeMonths or DashboardTimeRange.SixMonths => "Günlük alınan servis sayısı",
                DashboardTimeRange.OneYear or DashboardTimeRange.FiveYears => "Aylık alınan servis sayısı",
                DashboardTimeRange.All => "Tüm kayıtlar (aylık)",
                _ => "Zaman çizelgesi"
            };

            return vm;
        }

        private async Task<List<DashboardChartPoint>> BuildTimelineAsync(
            string rangeKey,
            DateTime? fromUtc,
            bool allTime,
            DateTime utcNow)
        {
            if (rangeKey == DashboardTimeRange.OneDay)
            {
                var windowStart = utcNow.AddDays(-1);
                var dates = await ServicesInRange(windowStart, false)
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
                var rows = await ServicesInRange(startDay, false)
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
                var endDay = utcNow.Date;
                var rows = await ServicesInRange(fromUtc, false)
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
                var rows = await ServicesInRange(fromUtc, false)
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
