using System.ComponentModel.DataAnnotations;

namespace RideShareAPI.Models.DTO;

public class RegistrationRequestDTO
{
    public string FirstName { get; set; }
    public  string LastName { get; set; }
    public string  PhoneNumber { get; set; }
    public User.Roles Role { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    
}