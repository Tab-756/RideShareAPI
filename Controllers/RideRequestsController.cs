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
public class RideRequestsController:ControllerBase
{
     private readonly IRideRequestRepository _rideRequestRepository;
    private readonly IUserRepository _userRepository;
    protected APIResponse _response;
    private readonly IMapper _mapper;

    public RideRequestsController(IRideRequestRepository rideRequestRepository,IMapper mapper,IUserRepository userRepository)
    {
        _rideRequestRepository = rideRequestRepository;
        _userRepository = userRepository;
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

            var rideRequest = await _rideRequestRepository.GetAsync(u => u.RideId == RideId);
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
            
            if (rideRequests == null)
            {
                return NotFound();
            }
            var sortedRequests =rideRequests.OrderByDescending(p => p.RideRequestId).ToList();
            
           List<RideRequestDTO>  rideRequestDto =_mapper.Map<List<RideRequestDTO>>(sortedRequests);

           foreach (var rideRequest in rideRequestDto)
           {
               var user = await _userRepository.GetAsync(u => u.UserId == rideRequest.RiderId);
               rideRequest.Rider = user.FirstName + " " + user.LastName;
               rideRequest.PhoneNumber = user.PhoneNumber; 
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


}