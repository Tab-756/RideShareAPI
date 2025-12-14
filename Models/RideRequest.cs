using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RideShareAPI.Models;

public class RideRequest
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RideRequestId { get; set; }
    [Required]
    [ForeignKey("Ride")]
    public int RideId { get; set; }
    [Required]
    [ForeignKey("User")]
    public int RiderId { get; set; }
    [Required] 
    public int NumberOfSeats { get; set; } = 1;

    [Required] 
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    [Required]
    public string Pickup { get; set; }
    [Required]
    public string Dropoff { get; set; }

    [Required] 
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public Ride Ride { get; set; } 
    public User User { get; set; } 
    
    
    
    
    public enum RequestStatus
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled,
        Completed
    }
    




}