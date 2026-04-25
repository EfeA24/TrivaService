using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrivaService.Data;
using TrivaService.Models;

namespace TrivaService.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await BuildDashboardAsync();
            return View(vm);
        }

        private async Task<DashboardViewModel> BuildDashboardAsync()
        {
            var vm = new DashboardViewModel();

            vm.CompletedServices = await _db.Services.CountAsync(s => s.Status == "Completed");
            vm.IncompleteServices = await _db.Services.CountAsync(s =>
                s.Status != "Completed" && s.Status != "Cancelled");
            vm.CancelledServices = await _db.Services.CountAsync(s => s.Status == "Cancelled");
            vm.TotalServices = await _db.Services.CountAsync();

            vm.ActiveCustomers = await _db.Customers.CountAsync(c => c.IsActive);
            vm.InactiveCustomers = await _db.Customers.CountAsync(c => !c.IsActive);
            vm.TotalCustomers = await _db.Customers.CountAsync();

            vm.TotalItems = await _db.Items.CountAsync();
            vm.TotalServiceItemLines = await _db.ServiceItems.CountAsync();

            vm.PaymentCompleteCount = await _db.Services.CountAsync(s => s.IsPaymentComplete);
            vm.PaymentIncompleteCount = await _db.Services.CountAsync(s => !s.IsPaymentComplete);

            vm.SupplierUsageInServices = await (
                from si in _db.ServiceItems
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

            var perCustomer = await _db.Services
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

            var unassigned = await _db.Services.CountAsync(s => s.CustomerId == null);
            if (unassigned > 0)
            {
                vm.ServicesPerCustomer.Add(new DashboardChartPoint
                {
                    Label = "Müşteri atanmamış",
                    Value = unassigned
                });
            }

            vm.ServicesByStatus = await _db.Services
                .GroupBy(s => s.Status)
                .Select(g => new DashboardChartPoint { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(-11);
            var monthlyRaw = await _db.Services
                .Where(s => s.ReceivedDate >= monthStart)
                .GroupBy(s => new { s.ReceivedDate.Year, s.ReceivedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var monthDict = monthlyRaw.ToDictionary(
                x => (x.Year, x.Month),
                x => x.Count);

            vm.ServicesReceivedByMonth = new List<DashboardChartPoint>();
            for (var i = 0; i < 12; i++)
            {
                var d = monthStart.AddMonths(i);
                var key = (d.Year, d.Month);
                monthDict.TryGetValue(key, out var cnt);
                vm.ServicesReceivedByMonth.Add(new DashboardChartPoint
                {
                    Label = d.ToString("yyyy-MM"),
                    Value = cnt
                });
            }

            return vm;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
