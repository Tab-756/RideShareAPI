using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RideShareAPI.Models;

public class Driver
{
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DriverId { get; set; }
    [Required]
    public string RegistrationNumber{ get; set; }
    [Required]
    public string Description { get; set; }
    [Required] 
    public bool IsAvailable { get; set; } = true;
}