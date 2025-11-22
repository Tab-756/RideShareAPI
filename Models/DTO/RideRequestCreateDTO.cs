namespace RideShareAPI.Models.DTO;

public class RideRequestCreateDTO
{
    public int RideId { get; set; }
    public int RiderId { get; set; }
    public int NumberOfSeats { get; set; } = 1;
}