using AutoMapper;
using City_Web.Models.Data;
using CityWeb.Auth.Model;
using CityWeb.Dto;
using CityWeb.Model.Data;

namespace CityWeb.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<CityDto, City>();
            CreateMap<City, CityDto>();
            CreateMap<City,IEnumerable<CityDto>>();
            CreateMap<WeatherData,WeatherDto>();
            CreateMap<CountryDto, Country>();
            CreateMap<Country, CountryDto>();
        }
    }
}
