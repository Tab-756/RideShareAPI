using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RideShareAPI.Models;
using RideShareAPI.Models.DTO;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RidesController:ControllerBase
{
    private readonly IRideRepository _rideRepository;
    private readonly IUserRepository _userRepository;
    protected APIResponse _response;
    private readonly IMapper _mapper;

    public RidesController(IRideRepository rideRepository, IUserRepository userRepository, IMapper mapper)
    {
        _rideRepository = rideRepository;
        _userRepository = userRepository;
        this._response = new();
        _mapper = mapper;

    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<APIResponse>> CreateRideAsync([FromBody] RideCreateDTO createDto )
    {
        try
        {
            if (createDto == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest("Invalid Ride");
            }

            if (await _rideRepository.GetAsync(u =>
                    (u.Status == RideStatus.Scheduled || u.Status == RideStatus.InProgress) &&
                    u.DriverId == createDto.DriverId)!=null)
            {
                ModelState.AddModelError("customError","You alreaady have an Active Ride");
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(ModelState);
            }

            Ride ride = _mapper.Map<Ride>(createDto);
            await _rideRepository.CreateAsync(ride);
            await _rideRepository.SaveAsync();

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = ride;

            return CreatedAtRoute("GetRideAsync", new { id = ride.RideId }, _response);

        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.Errors.Add(e.ToString());
        }

        return _response;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]

    public async Task<ActionResult<APIResponse>> GetAllRidesAsync()
    {
        try
        {
            IEnumerable<Ride> rides = await _rideRepository.GetAllAsync(tracked:false);
            if (rides == null) BadRequest();

            List<RideDTO> rideDtos = new List<RideDTO>();
            RideDTO rideDto = new RideDTO();

            foreach (var ride in rides)
            {
                rideDto = _mapper.Map<RideDTO>(ride);
                var user = await _userRepository.GetAsync(u => u.UserId == ride.DriverId);
                rideDto.Driver = user.FirstName + " " + user.LastName;
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                rideDto.PhoneNumber = user.PhoneNumber;
                
                rideDtos.Add(rideDto);
                }

            _response.Result = rideDtos;
            return _response;


        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.Errors.Add(e.ToString());
        }

        return _response;
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(400)]
    [ProducesResponseType(204)]

    public async Task<ActionResult<APIResponse>> UpdateRideAsync(int id, [FromBody] RideUpdateDTO updateDto)
    {
        try
        {
            if (id == 0 || updateDto.RideId != id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            Ride ride = _mapper.Map<Ride>(updateDto);
            await _rideRepository.UpdateAsync(ride);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);

        }
        catch (Exception e)
        {
            _response.Errors.Add(e.ToString());
            _response.StatusCode = HttpStatusCode.BadRequest;
        }

        return (_response);
    }
    
    [HttpGet("{id}", Name = "GetRideAsync")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<APIResponse>> GetRideAsync(int id)
    {

        try
        {
            if (id == 0)
            {
                return BadRequest("Invalid Id");
            }

            var ride = await _rideRepository.GetAsync(u => u.RideId == id);
            if (ride == null)
            {
                return NotFound();
            }

            _response.Result = _mapper.Map<RideUpdateDTO>(ride);
            _response.StatusCode = HttpStatusCode.OK;

            return Ok(_response);


        }
        catch (Exception e)
        {
            _response.Errors.Add(e.ToString());
            _response.StatusCode = HttpStatusCode.BadRequest;
        }

        return _response;
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(400)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(200)]


        
    public async Task<ActionResult<APIResponse>> DeleteRideAsync(int id)
    {
        try
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest();
            }

            var ride = await _rideRepository.GetAsync(u => u.RideId == id);

            if (ride == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NoContent();
            }

            await _rideRepository.RemoveAsync(ride);
            await _rideRepository.SaveAsync();
            _response.StatusCode = HttpStatusCode.NoContent;

            return Ok(_response);


        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.Errors.Add(e.ToString());
                
        }

        return _response;
    }


}