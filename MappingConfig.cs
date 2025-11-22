using AutoMapper;
using RideShareAPI.Models;
using RideShareAPI.Models.DTO;

namespace RideShareAPI;

public class MappingConfig:Profile
{
    public MappingConfig()
    {
        CreateMap<Ride, RideDTO>();
        CreateMap<RideDTO, Ride>();
        CreateMap<Ride, RideCreateDTO>().ReverseMap();
        CreateMap<Ride, RideUpdateDTO>().ReverseMap();
        CreateMap<RideRequest, RideRequestDTO>();
        CreateMap<RideRequestDTO, RideRequest>();
        CreateMap<RideRequest, RideRequestDTO>().ReverseMap();
        CreateMap<RideRequest, RideRequestUpdateDTO>().ReverseMap();
    }
}