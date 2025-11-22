using RideShareAPI.Data;
using RideShareAPI.Models;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Repository;

public class RideRequestRepository:Repository<RideRequest>,IRideRequestRepository
{
    private readonly ApplicationDbContext _db;
    public RideRequestRepository(ApplicationDbContext db):base(db)
    {
        _db = db;

    }
}