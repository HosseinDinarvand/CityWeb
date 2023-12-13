using System.ComponentModel.DataAnnotations;

namespace CityWeb.Model.Data
{
    public class WeatherData
    {
        [Key]
        public int id { get; set; }
        public string main { get; set; } = String.Empty;
        public string description { get; set; } = String.Empty;
        public string icon { get; set; } = String.Empty;
        public int CityID { get; set; }
    }
}
