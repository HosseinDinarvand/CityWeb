using City_Web.Models.Data;
using WebApplication10.Models.Interface;

namespace WebApplication10.Models.Repository
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ApplicationDbContext _context;

        public CountryRepository(ApplicationDbContext context)
        {
            this._context = context;
        }


        public Country getByCountryName(string countryName)
        {
            return _context.country.Where(c => c.Name == countryName).FirstOrDefault();
        }

        public bool add(Country country)
        {
            _context.Add(country);
            return save();
        }

        public bool save()
        {
            var save = _context.SaveChanges();
            return save > 0 ? true : false;
        }
    }
}
