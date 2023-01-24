using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PaylodeWeather.Core.DTOs;

namespace PaylodeWeather.Core.Utilities
{
    public class WeatherProfile : Profile
    {
        public WeatherProfile()
        {
            CreateMap<IdentityUser, UserDTO>();
        }
    }
}
