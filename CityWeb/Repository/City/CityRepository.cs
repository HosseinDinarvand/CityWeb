using City_Web.Models.Data;
using CityWeb.Dto;
using Microsoft.EntityFrameworkCore;

namespace WebApplication10.Models.Repository
{
    public class CityRepository : ICityRepository
    {
        private readonly ApplicationDbContext _context;

        public CityRepository(ApplicationDbContext context)
        {
            this._context = context;
        }
        public bool add(City city)
        {
            _context.Add(city);
            return save();
        }

        public bool delete(City city)
        {
            _context.Remove(city);
            return save();
        }

        public  List<City> getCities()
        {
            return _context.city.OrderBy(c=>c.Id).ToList();
        }

        public City getByCityName(string cityName)
        {
            return _context.city.Where(c => c.Name == cityName).FirstOrDefault();
        }

        public City getByCityNameNoTracking(string cityName)
        {
            return _context.city.AsNoTracking().FirstOrDefault(c=>c.Name==cityName);
        }
        public  IEnumerable<City> getCityByContryName(string countryName)
        {
            return  _context.city.Where(c => c.CountryName == countryName).ToList();
        }
        public bool existCity(string name)
        {
            return _context.city.Any(c => c.Name == name);
        }
        public bool update(City city)
        {
            _context.Update(city);
            return save();
        }

        public bool save()
        {
            var save = _context.SaveChanges();
            return save > 0 ? true : false;
        }

    }
}
