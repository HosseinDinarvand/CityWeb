

namespace CityWeb.Dto
{
    public class CityDto
    {
        public string Name { get; set; } = String.Empty;
        public long Population { get; set; }
        public string CountryName { get; set; } = String.Empty;
        public string CurrentWeather { get; set; } = String.Empty;
        public DateTime TimeUpdateWeather { get; set; } 
        public int WeatherId { get; set; }
    }
}
