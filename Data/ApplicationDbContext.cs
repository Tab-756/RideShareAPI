using Microsoft.EntityFrameworkCore;
using RideShareAPI.Models;

namespace RideShareAPI.Data;

public class ApplicationDbContext:DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
    {
        
        
    }
    public DbSet<Driver> Drivers { get; set; }
}