namespace RideShareAPI.Models.DTO;

public class RideRequestDTO
{
    public int RideRequestId { get; set; }
    public int RideId { get; set; }
    public int RiderId { get; set; }
    public string Pickup { get; set; }
    public string Dropoff { get; set; }
    public int NumberOfSeats { get; set; } = 1;
    public RideRequest.RequestStatus Status { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string Rider { get; set; }
    public string PhoneNumber { get; set; }
    public string RegistrationNumber { get; set; }
}