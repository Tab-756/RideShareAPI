using Microsoft.EntityFrameworkCore;
using RideShareAPI.Models;

namespace RideShareAPI.Data;

public class ApplicationDbContext:DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
    {
        
        
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<RideRequest> RideRequests { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasMany(u => u.Rides).WithOne(r => r.User).HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>().HasMany(u => u.RideRequests).WithOne(r => r.User).HasForeignKey(r => r.RiderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}