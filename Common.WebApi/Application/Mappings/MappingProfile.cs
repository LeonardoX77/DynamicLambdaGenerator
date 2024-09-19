using AutoMapper;
using Common.Domain.Entities;
using Common.WebApi.Application.Models.Client;
using Common.WebApi.Application.Models.Photographer;
using Common.WebApi.Application.Models.Location;
using Common.WebApi.Application.Models.Session;

namespace Common.WebApi.Application.Mappings
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Client, ClientRequestDto>().ReverseMap();
            CreateMap<Client, ClientResponseDto>().ReverseMap();
            CreateMap<Photographer, PhotographerDto>().ReverseMap();
            CreateMap<Session, SessionRequestDto>().ReverseMap();
            CreateMap<Session, SessionResponseDto>().ReverseMap();
            CreateMap<Location, LocationDto>().ReverseMap();
        }
    }

}
