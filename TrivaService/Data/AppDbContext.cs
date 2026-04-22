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

            modelBuilder.Entity<Roles>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Roles)
                .HasForeignKey(u => u.RolesId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.Items)
                .WithOne(i => i.Supplier)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceItem>()
                .HasOne(si => si.Service)
                .WithMany(s => s.ServiceItems)
                .HasForeignKey(si => si.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceItem>()
                .HasOne(si => si.Item)
                .WithMany(i => i.ServiceItems)
                .HasForeignKey(si => si.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Services)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceVisuals>()
                .HasOne<Service>()
                .WithMany(s => s.ServiceVisuals)
                .HasForeignKey("ServiceId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
