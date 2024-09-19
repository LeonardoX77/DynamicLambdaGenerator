using AutoMapper;
using Common.Domain.Entities;
using Common.WebApi.Application.Models.Client;
using Common.WebApi.Application.Models.Location;
using Common.WebApi.Application.Models.Photographer;
using System.Reflection;

namespace Common.Tests.Infrastructure.AutoMoq
{
    internal class MappingProfile : Profile
    {
        private Assembly assembly;

        public MappingProfile(Assembly assembly)
        {
            this.assembly = assembly;
            CreateMap<Client, ClientRequestDto>().ReverseMap();
            CreateMap<Client, ClientResponseDto>().ReverseMap();
            CreateMap<Photographer, PhotographerDto>().ReverseMap();
            CreateMap<Session, SessionResponseDto>().ReverseMap();
            CreateMap<Session, SessionResponseDto>().ReverseMap();
            CreateMap<Location, LocationDto>().ReverseMap();
        }
    }
}
