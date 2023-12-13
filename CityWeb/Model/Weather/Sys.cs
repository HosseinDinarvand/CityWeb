namespace CityWeb.Model.Weather
{
    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; } = String.Empty;
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }
}
