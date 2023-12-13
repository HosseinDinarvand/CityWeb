namespace CityWeb.Model.Weather
{
    public class DataContainer
    {
        public Clouds Clouds { get; set; }
        public Coord Coord { get; set; }
        public Main Main { get; set; }
        public Root Root { get; set; }
        public Sys Sys { get; set; }
        public List<Weather> Weather { get; set; }
        public Wind Wind { get; set; }
    }
}
