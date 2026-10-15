using Microsoft.EntityFrameworkCore;
using mvc7_2.Models;

namespace mvc7_2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.AccessFailedCount).HasDefaultValue(0);
            entity.Property(u => u.IsLocked).HasDefaultValue(false);
        });

        // tblUsers 1 - N tblUserRoles
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasOne(r => r.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => new { r.UserId, r.RoleName }).IsUnique();

            // RoleName chỉ nhận 1 trong 3 giá trị cố định
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_tblUserRoles_RoleName", "RoleName IN ('NORMAL', 'STAFF', 'MANAGER')"));
        });

        // tblUsers 1 - N tblProducts
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.CreatedByUser)
                  .WithMany(u => u.Products)
                  .HasForeignKey(p => p.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
