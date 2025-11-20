using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RideShareAPI.Data;
using RideShareAPI.Models;
using RideShareAPI.Models.DTO;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Repository;

public class UserRepository:IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher;
    private string secretKey;
    
    public UserRepository(ApplicationDbContext db,IConfiguration configuration)
    {
        _db = db;
        _passwordHasher = new PasswordHasher<User>();
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");

    }
    public bool IsUniqueUser(string email,string phoneNumber)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email && u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            return true;
        }
        return false;
        
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto)
    {
        try
        {
            var user =  await _db.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDto.EmailOrPhoneNumber||u.PhoneNumber==loginRequestDto.EmailOrPhoneNumber);
            if (user == null) return FailedLogin();
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginRequestDto.Password);
            if (result == PasswordVerificationResult.Failed) return FailedLogin();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDto = new LoginResponseDTO()
            {
                Token = tokenHandler.WriteToken(token),
                User = user
            };
            return loginResponseDto;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Errror verifying password: {e.Message}");
            return FailedLogin();
        }
          
    }

    public async Task<User> Register(RegistrationRequestDTO registrationRequestDto)
    {
        User user = new User()
        {
            FirstName = registrationRequestDto.FirstName,
            LastName = registrationRequestDto.LastName,
            PhoneNumber = registrationRequestDto.PhoneNumber,
            Email = registrationRequestDto.Email,
            Role = registrationRequestDto.Role

        };
        user.Password = _passwordHasher.HashPassword(user, registrationRequestDto.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        user.Password = "";
        return user;
    }
    private LoginResponseDTO FailedLogin()
    {
        return new LoginResponseDTO
        {
            Token = "",
            User = null
        };
    }
}