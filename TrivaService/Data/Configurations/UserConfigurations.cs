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

            builder.HasMany(r => r.EntityPermissions)
                .WithOne(p => p.Role)
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
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

    public class RoleEntityPermissionConfiguration : IEntityTypeConfiguration<RoleEntityPermission>
    {
        public void Configure(EntityTypeBuilder<RoleEntityPermission> builder)
        {
            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(x => new { x.RoleId, x.EntityName }).IsUnique();

            builder.HasMany(x => x.PropertyPermissions)
                .WithOne(p => p.RoleEntityPermission)
                .HasForeignKey(p => p.RoleEntityPermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class RolePropertyPermissionConfiguration : IEntityTypeConfiguration<RolePropertyPermission>
    {
        public void Configure(EntityTypeBuilder<RolePropertyPermission> builder)
        {
            builder.Property(x => x.PropertyName)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(x => new { x.RoleEntityPermissionId, x.PropertyName }).IsUnique();
        }
    }
}
