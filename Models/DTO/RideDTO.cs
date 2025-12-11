namespace RideShareAPI.Models.DTO;

public class RideDTO
{
    public int RideId { get; set; }
    public int DriverId { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public DateTime StartTime { get; set; }
    public decimal PricePerSeat { get; set; }
    public int AvailableSeats { get; set; }
    public RideStatus Status { get; set; } = RideStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string RegistrationNumber { get; set; }
    public string Description { get; set; }
    public string Driver { get; set; }
    public string PhoneNumber { get; set; }
}