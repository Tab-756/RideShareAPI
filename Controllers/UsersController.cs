using System.Net;

using Microsoft.AspNetCore.Mvc;
using RideShareAPI.Models;
using RideShareAPI.Models.DTO;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Controllers;
[Route("api/UsersAuth")]
[ApiController]
public class UsersController:ControllerBase
{
    protected APIResponse _response;
    private readonly IUserRepository _userRepo;
    public UsersController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
        this._response = new();
    }

    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
    {
        var loginResponse = await _userRepo.Login(model);
        if (loginResponse.User == null || string.IsNullOrEmpty(loginResponse.Token))
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Errors.Add("Incorrect Login Credentials");
            return BadRequest(_response);
        }

        _response.StatusCode = HttpStatusCode.OK;
        _response.Result = loginResponse;
        return Ok(_response);

    }

    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
    {
        bool ifUniqueUser = _userRepo.IsUniqueUser(model.Email,model.PhoneNumber);
        if (!ifUniqueUser)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Errors.Add("User already exists ");
            return BadRequest(_response);
            }

        var user = await _userRepo.Register(model);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Errors.Add("Error while Registering");
            return BadRequest(_response);
        }

        _response.StatusCode = HttpStatusCode.OK;
        return Ok(_response);
    }
}