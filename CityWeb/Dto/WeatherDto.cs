namespace CityWeb.Dto
{
    public class WeatherDto
    {
        public string main { get; set; } = String.Empty;
        public string description { get; set; } = String.Empty;
        public string icon { get; set; } = String.Empty;
        public int CityID { get; set; }
    }
}
