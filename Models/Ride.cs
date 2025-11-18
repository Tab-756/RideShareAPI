using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RideShareAPI.Models;

public class Ride
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RideId { get; set; }
    
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    public string Source { get; set; }

    [Required]
    public string Destination { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public decimal PricePerSeat { get; set; }

    [Required]
    public RideStatus Status { get; set; } = RideStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string RegistrationNumber { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public bool IsAvailable { get; set; } = true;
    
    public User User { get; set; }
}



public enum RideStatus
{
    Scheduled,   // 0
    InProgress,  // 1
    Completed,   // 2
    Cancelled    // 3
}
