using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using RideShareAPI.Migrations;
using RideShareAPI.Models;
using RideShareAPI.Models.DTO;
using RideShareAPI.Repository;
using RideShareAPI.Repository.IRepository;

namespace RideShareAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RideRequestsController:ControllerBase
{
     private readonly IRideRequestRepository _rideRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRideRepository _rideRepository;
    protected APIResponse _response;
    private readonly IMapper _mapper;

    public RideRequestsController(IRideRequestRepository rideRequestRepository,IMapper mapper,IUserRepository userRepository,IRideRepository rideRepository)
    {
        _rideRequestRepository = rideRequestRepository;
        _userRepository = userRepository;
        _rideRepository = rideRepository;
        this._response = new();
        _mapper = mapper;

    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<APIResponse>> CreateRideAsync([FromBody] RideRequestCreateDTO createDto )
    {
        try
        {
            if (createDto == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest("Invalid Ride");
            }

            if (await _rideRequestRepository.GetAsync(u =>
                    (u.Status ==RideRequest.RequestStatus.Accepted || u.Status==RideRequest.RequestStatus.Pending) &&
                    u.RiderId == createDto.RiderId)!=null)
            {
                ModelState.AddModelError("customError","You alreaady have an Active Ride Request");
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(ModelState);
            }

            RideRequest rideRequest = _mapper.Map<RideRequest>(createDto);
            await _rideRequestRepository.CreateAsync(rideRequest);
            await _rideRequestRepository.SaveAsync();

            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = rideRequest;

            return CreatedAtRoute("GetRideRequestAsync", new { id = rideRequest.RideRequestId }, _response);

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

    public async Task<ActionResult<APIResponse>> GetAllRideRequestsAsync()
    {
        try
        {
            IEnumerable<RideRequest> rideRequests = await _rideRequestRepository.GetAllAsync(tracked:false);
            if (rideRequests == null) BadRequest();

            List<RideRequestDTO> rideRequestDtos = new List<RideRequestDTO>();
            RideRequestDTO rideRequestDto = new RideRequestDTO();

            foreach (var rideRequest in rideRequests)
            {
                rideRequestDto = _mapper.Map<RideRequestDTO>(rideRequest);
                var user = await _userRepository.GetAsync(u => u.UserId == rideRequest.RiderId);
                rideRequestDto.Rider = user.FirstName + " " + user.LastName;
                rideRequestDto.PhoneNumber = user.PhoneNumber;
                
                rideRequestDtos.Add(rideRequestDto);
                }

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = rideRequestDtos;
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

    public async Task<ActionResult<APIResponse>> UpdateRideRequestAsync(int id, [FromBody] RideRequestUpdateDTO updateDto)
    {
        try
        {
            if (id == 0 || updateDto.RideRequestId != id)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
           

            RideRequest rideRequest = _mapper.Map<RideRequest>(updateDto);
            await _rideRequestRepository.UpdateAsync(rideRequest);
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
    
    [HttpGet("{id}", Name = "GetRideRequestAsync")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<APIResponse>> GetRideRequestAsync(int id)
    {

        try
        {
            if (id == 0)
            {
                return BadRequest("Invalid Id");
            }

            var rideRequest = await _rideRequestRepository.GetAsync(u => u.RideRequestId == id);
            if (rideRequest == null)
            {
                return NotFound();
            }
            
            
            RideRequestDTO rideRequestDto =_mapper.Map<RideRequestDTO>(rideRequest);
            var user = await _userRepository.GetAsync(u => u.UserId == rideRequest.RiderId);
            rideRequestDto.Rider = user.FirstName + " " + user.LastName;
            rideRequestDto.PhoneNumber = user.PhoneNumber;


            _response.Result = rideRequestDto;
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
    
    [HttpGet("ride/{RideId}", Name = "GetRideRequestByRideIdAsync")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<APIResponse>> GetRideRequestByRideIdAsync(int RideId)
    {

        try
        {
            if (RideId == 0)
            {
                return BadRequest("Invalid Id");
            }

            var rideRequests = await _rideRequestRepository.GetAllAsync(
                filter: u => u.RideId == RideId && u.Status != RideRequest.RequestStatus.Cancelled
            );
            if (rideRequests == null || rideRequests.Count == 0)
            {
                return NotFound();
            }
            
            List<RideRequestDTO> rideRequestDtos = new List<RideRequestDTO>();

            foreach (var rideRequest in rideRequests)
            {
                RideRequestDTO rideRequestDto = _mapper.Map<RideRequestDTO>(rideRequest);
                var user = await _userRepository.GetAsync(u => u.UserId == rideRequest.RiderId);
                rideRequestDto.Rider = user.FirstName + " " + user.LastName;
                rideRequestDto.PhoneNumber = user.PhoneNumber;
                
                var ride = await _rideRepository.GetAsync(r => r.RideId == rideRequest.RideId);
                rideRequestDto.RegistrationNumber = ride.RegistrationNumber;

                rideRequestDtos.Add(rideRequestDto);
            }

            _response.Result = rideRequestDtos;
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

     
    [HttpGet("requests/{RiderId}", Name = "GetRiderRequestsByRiderIdAsync")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<APIResponse>> GetRideRequestsByRiderIdAsync(int RiderId)
    {

        try
        {
            if (RiderId == 0)
            {
                return BadRequest("Invalid Id");
            }

            var rideRequests = await _rideRequestRepository.GetAllAsync(u => u.RiderId == RiderId);
            
            if (rideRequests == null || rideRequests.Count == 0)
            {
                return NotFound();
            }
            var sortedRequests = rideRequests.OrderByDescending(p => p.RideRequestId).Take(5).ToList();
            
           List<RideRequestDTO>  rideRequestDto =_mapper.Map<List<RideRequestDTO>>(sortedRequests);

           foreach (var rideRequest in rideRequestDto)
           {
               var user = await _userRepository.GetAsync(u => u.UserId == rideRequest.RiderId);
               rideRequest.Rider = user.FirstName + " " + user.LastName;
               rideRequest.PhoneNumber = user.PhoneNumber;
               
               var ride = await _rideRepository.GetAsync(r => r.RideId == rideRequest.RideId);
               rideRequest.RegistrationNumber = ride.RegistrationNumber;
           }
          
            


            _response.Result = rideRequestDto;
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
    public async Task<ActionResult<APIResponse>> DeleteRideRequestAsync(int id)
    {
        try
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest();
            }

            var rideRequest = await _rideRequestRepository.GetAsync(u => u.RideRequestId == id);

            if (rideRequest == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NoContent();
            }

            await _rideRequestRepository.RemoveAsync(rideRequest);
            await _rideRequestRepository.SaveAsync();
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
    
    [HttpPatch("{RideRequestId}")]
    [ProducesResponseType(400)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdatePartialRideAndRequest( JsonPatchDocument <RideRequestUpdateDTO> patchDto)
    {
        // Extract RideRequestId from route
        if (!HttpContext.Request.RouteValues.TryGetValue("RideRequestId", out var rideRequestIdObj)
            || !int.TryParse(rideRequestIdObj?.ToString(), out int rideRequestId))
        {
            return BadRequest("Invalid RideRequestId in URL.");
        }
        if (patchDto == null )
        {
            return BadRequest();
        }

        var rideRequest = await _rideRequestRepository.GetAsync(u => u.RideRequestId == rideRequestId, tracked: false);

        RideRequestUpdateDTO rideRequestUpdateDto = _mapper.Map<RideRequestUpdateDTO>(rideRequest);
        patchDto.ApplyTo(rideRequestUpdateDto, ModelState);
        
       
        
        RideRequest model = _mapper.Map<RideRequest>(rideRequestUpdateDto);
        
        var ride = await _rideRepository.GetAsync(u => u.RideId == rideRequestUpdateDto.RideId, tracked: false);

        RideUpdateDTO rideUpdateDto = _mapper.Map<RideUpdateDTO>(ride);
        var remainingSeats = rideUpdateDto.AvailableSeats;
        var rideStatus = rideUpdateDto.Status;

        if (rideRequestUpdateDto.Status == RideRequest.RequestStatus.Accepted)
        {
            remainingSeats = rideUpdateDto.AvailableSeats - rideRequest.NumberOfSeats;
            if (remainingSeats < 0)
            {
                return BadRequest("Requested seats exceed available seats");
            }

            if (remainingSeats == 0)
            {
                rideStatus = RideStatus.Full;
            }
        }

        JsonPatchDocument<RideUpdateDTO> patchRideDto = new();
        patchRideDto.Replace(r => r.AvailableSeats, remainingSeats);
        patchRideDto.Replace(r => r.Status, rideStatus);
        patchRideDto.ApplyTo(rideUpdateDto, ModelState);
        Ride rideModel = _mapper.Map<Ride>(rideUpdateDto);
        
        if (remainingSeats == 0 && rideRequestUpdateDto.Status == RideRequest.RequestStatus.Accepted)
        {
            rideModel.IsAvailable = false;
        }
        
        await _rideRepository.UpdateAsync(rideModel);
        await _rideRequestRepository.UpdateAsync(model);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
                
        }

        return NoContent();

    }



}