using CityWeb.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace City_Web.Models.Data
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public long Population { get; set; }
        public string CountryName { get; set; } = String.Empty;
        public DateTime TimeUpdateWeather { get; set; }
        public int? countryId { get; set; }
        public int WeatherId { get; set; }
        public string CurrentWeather { get; set; } = String.Empty;
        public virtual string Weather { get; set; } = String.Empty;
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
