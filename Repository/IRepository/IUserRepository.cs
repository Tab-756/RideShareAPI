using RideShareAPI.Models;
using RideShareAPI.Models.DTO;

namespace RideShareAPI.Repository.IRepository;

public interface IUserRepository
{
    
        bool IsUniqueUser(string email, string phoneNumber);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto);
        Task<User> Register(RegistrationRequestDTO registrationRequestDto);
    
}