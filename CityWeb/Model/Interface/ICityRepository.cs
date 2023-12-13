using City_Web.Models.Data;
using CityWeb.Dto;

namespace WebApplication10.Models.Repository
{
    public interface ICityRepository
    {
        List<City> getCities();
        IEnumerable<City> getCityByContryName(string countrytName);
        City getByCityName(string cityName);
        City getByCityNameNoTracking(string cityName);
        bool existCity(string name);
        bool add(City city);
        bool update(City city);
        bool delete(City city);
        bool save();
    }
}
