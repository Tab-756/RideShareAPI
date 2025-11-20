namespace RideShareAPI.Models.DTO;

public class LoginRequestDTO
{
    public string EmailOrPhoneNumber { get; set; }
    public string Password { get; set; }
}