using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
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
    private readonly IRideRequestRepository _rideRequestRepository;
    protected APIResponse _response;
    private readonly IMapper _mapper;

    public RidesController(IRideRepository rideRepository, IUserRepository userRepository, IMapper mapper, IRideRequestRepository rideRequestRepository)
    {
        _rideRepository = rideRepository;
        _userRepository = userRepository;
        _rideRequestRepository = rideRequestRepository;
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
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);


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
    [HttpGet("driver/{driverId}", Name = "GetRideByDriverIdAsync")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<APIResponse>> GetRideByDriverIdAsync(int driverId)
    {

        try
        {
            if (driverId == 0)
            {
                return BadRequest("Invalid Id");
            }

            var rides = await _rideRepository.GetAllAsync(u => u.DriverId == driverId, tracked: false);
            if (rides == null || rides.Count == 0)
            {
                return NotFound();
            }

            var lastFiveRides = rides.OrderByDescending(r => r.CreatedAt).Take(5).ToList();
            
            List<RideDTO> rideDtos = new List<RideDTO>();
            var driver = await _userRepository.GetAsync(u => u.UserId == driverId);

            foreach (var ride in lastFiveRides)
            {
                var rideDto = _mapper.Map<RideDTO>(ride);
                rideDto.PhoneNumber = driver.PhoneNumber;
                rideDto.Driver = driver.FirstName + " " + driver.LastName;
                rideDtos.Add(rideDto);
            }

            _response.Result = rideDtos;
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

    [HttpPatch("{id}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<APIResponse>> UpdateRideStatusAsync([FromBody] JsonPatchDocument<RideStatusUpdateDTO> patchDoc)
    {
        try
        {
            if (!HttpContext.Request.RouteValues.TryGetValue("id", out var rideIdObj)
                || !int.TryParse(rideIdObj?.ToString(), out int rideId))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add("Invalid ride ID in URL.");
                return BadRequest(_response);
            }

            if (patchDoc == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add("Patch document is null");
                return BadRequest(_response);
            }

            var ride = await _rideRepository.GetAsync(u => u.RideId == rideId);

            if (ride == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Errors.Add("Ride not found");
                return NotFound(_response);
            }

            var statusUpdateDto = new RideStatusUpdateDTO { Status = ride.Status };
            patchDoc.ApplyTo(statusUpdateDto, ModelState);

            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add("Invalid patch document");
                return BadRequest(_response);
            }

            if (statusUpdateDto.Status == RideStatus.Cancelled)
            {
                var activeRequests = await _rideRequestRepository.GetAllAsync(
                    filter: r => r.RideId == rideId && 
                                 (r.Status == RideRequest.RequestStatus.Accepted || 
                                  r.Status == RideRequest.RequestStatus.Pending),
                    tracked: true
                );

                if (activeRequests != null && activeRequests.Count > 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors.Add("Cannot cancel ride with active requests. Please reject or cancel all pending/accepted requests first.");
                    return BadRequest(_response);
                }
            }

            ride.Status = statusUpdateDto.Status;

            if (statusUpdateDto.Status == RideStatus.Completed)
            {
                var acceptedRequests = await _rideRequestRepository.GetAllAsync(
                    filter: r => r.RideId == rideId && r.Status == RideRequest.RequestStatus.Accepted,
                    tracked: true
                );

                if (acceptedRequests != null && acceptedRequests.Count > 0)
                {
                    foreach (var request in acceptedRequests)
                    {
                        request.Status = RideRequest.RequestStatus.Completed;
                        await _rideRequestRepository.UpdateAsync(request);
                    }
                }

                ride.IsAvailable = false;
            }

            if (statusUpdateDto.Status == RideStatus.Cancelled)
            {
                ride.IsAvailable = false;
            }

            if (ride.AvailableSeats == 0)
            {
                ride.IsAvailable = false;
            }

            await _rideRepository.UpdateAsync(ride);
            await _rideRepository.SaveAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<RideDTO>(ride);

            return Ok(_response);
        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.Errors.Add(e.ToString());
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