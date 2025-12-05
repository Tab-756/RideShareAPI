namespace RideShareAPI.Models.DTO;

public class RideRequestUpdateDTO
{
    public int RideRequestId { get; set; }
    public int RideId { get; set; }
    public int RiderId { get; set; }
    public RideRequest.RequestStatus Status { get; set; }
}