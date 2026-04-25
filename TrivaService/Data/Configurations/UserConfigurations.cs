using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrivaService.Models.UserEntities;

namespace TrivaService.Data.Configurations
{
    public class UserConfigurations : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            builder.HasMany(r => r.Users)
                .WithOne(u => u.Roles)
                .HasForeignKey(u => u.RolesId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class UsersEntityConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.Property(u => u.UserName)
                .IsRequired();

            builder.Property(u => u.UserPasswordHash)
                .IsRequired();
        }
    }
}
