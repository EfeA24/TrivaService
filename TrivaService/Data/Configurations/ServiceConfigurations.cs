using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrivaService.Models.ServiceEntites;

namespace TrivaService.Data.Configurations
{
    public class ServiceConfigurations : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.Property(s => s.ServiceCode)
                .IsRequired();

            builder.Property(s => s.FaultDescription)
                .IsRequired();

            builder.Property(s => s.Status)
                .IsRequired();

            builder.Property(s => s.EstimatedCost)
                .HasPrecision(18, 2);

            builder.Property(s => s.FinalCost)
                .HasPrecision(18, 2);
        }
    }

    public class CustomerEntityConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(c => c.CustomerName)
                .IsRequired();

            builder.Property(c => c.CustomerPhone)
                .IsRequired();

            builder.HasMany(c => c.Services)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ServiceItemEntityConfiguration : IEntityTypeConfiguration<ServiceItem>
    {
        public void Configure(EntityTypeBuilder<ServiceItem> builder)
        {
            builder.HasOne(si => si.Service)
                .WithMany(s => s.ServiceItems)
                .HasForeignKey(si => si.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(si => si.Item)
                .WithMany(i => i.ServiceItems)
                .HasForeignKey(si => si.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(si => si.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(si => si.UnitCost)
                .HasPrecision(18, 2);

            builder.Property(si => si.TotalPrice)
                .HasPrecision(18, 2);
        }
    }

    public class ServiceVisualsEntityConfiguration : IEntityTypeConfiguration<ServiceVisuals>
    {
        public void Configure(EntityTypeBuilder<ServiceVisuals> builder)
        {
            builder.Property(sv => sv.ServiceVisualName)
                .IsRequired();

            builder.HasOne<Service>()
                .WithMany(s => s.ServiceVisuals)
                .HasForeignKey(sv => sv.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
