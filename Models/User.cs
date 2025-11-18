using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RideShareAPI.Models;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public  string LastName { get; set; }
    [Required]
    public string  PhoneNumber { get; set; }

    [Required] public Roles Role { get; set; } = Roles.rider;
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } 
    
    public ICollection<Ride> Rides { get; set; }

    
    
    public enum Roles
    {
        driver, // 0
        rider  // 1
    }
    
    
    
    
    
}