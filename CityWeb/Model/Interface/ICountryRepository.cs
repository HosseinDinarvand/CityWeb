using City_Web.Models.Data;

namespace WebApplication10.Models.Interface
{
    public interface ICountryRepository
    {
        Country getByCountryName(string countryName);
        bool add(Country country);
        bool save();
    }
}
