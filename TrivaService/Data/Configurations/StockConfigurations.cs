using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrivaService.Models.StockEntities;

namespace TrivaService.Data.Configurations
{
    public class StockConfigurations : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.HasMany(s => s.Items)
                .WithOne(i => i.Supplier)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ItemEntityConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.Property(i => i.ItemName)
                .IsRequired();

            builder.Property(i => i.ItemPrice)
                .HasPrecision(18, 2);
        }
    }
}
