using MediQueue.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.Repository.Identity;

public class AppIdentityDbContext : IdentityDbContext<AppUser>
{
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
    {
    }

    public DbSet<Address> Addresses { get; set; }
    public DbSet<Otp> Otps { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Address>()
            .HasOne(a => a.AppUser)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
