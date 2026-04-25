using Microsoft.EntityFrameworkCore;
using TrivaService.Models.ServiceEntites;
using TrivaService.Models.StockEntities;
using TrivaService.Models.UserEntities;

namespace TrivaService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
        public DbSet<ServiceVisuals> ServiceVisuals => Set<ServiceVisuals>();

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();

        public DbSet<Roles> Roles => Set<Roles>();
        public DbSet<Users> Users => Set<Users>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
