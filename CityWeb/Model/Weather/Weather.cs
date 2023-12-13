namespace CityWeb.Model.Weather
{
    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; } = String.Empty;
        public string description { get; set; } = String.Empty;
        public string icon { get; set; } = String.Empty;
    }
}
