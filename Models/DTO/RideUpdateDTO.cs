namespace RideShareAPI.Models.DTO;

public class RideUpdateDTO
{
    public int RideId { get; set; }
    public int DriverId { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public DateTime StartTime { get; set; }
    public decimal PricePerSeat { get; set; }
    public int AvailableSeats { get; set; }
    public RideStatus Status { get; set; } = RideStatus.Scheduled;
    public string RegistrationNumber { get; set; }
    public string Description { get; set; }
}