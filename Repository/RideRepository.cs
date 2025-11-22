using RideShareAPI.Data;
using RideShareAPI.Models;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Repository;

public class RideRepository:Repository<Ride>,IRideRepository
{
    private readonly ApplicationDbContext _db;
    public RideRepository(ApplicationDbContext db):base(db)
    {
        _db= db;
    }
    
}