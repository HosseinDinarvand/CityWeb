using City_Web.Models.Data;
using CityWeb.Model.Data;
using CityWeb.Model.Interface;
using System.ComponentModel;

namespace CityWeb.Model.Repository
{
    public class WRepository : IWeatherRepository
    {
        private readonly ApplicationDbContext _context;
        public enum Weather
        {
            [Description("Warm and dry")]
            Warmanddry,
            [Description("Cold and mountain")]
            ColdMountain,
            [Description("Mild and humid")]
            MildAndHumid,
            [Description("Warm and humid")]
            WarmAndHumid
        }
        public WRepository(ApplicationDbContext context)
        {
            this._context = context;
        }
        public bool add(WeatherData weather)
        {
            _context.Add(weather);
            return save();
        }

        public bool save()
        {
            var save = _context.SaveChanges();
            return (save > 0) ? true : false;
        }
        public string setWeather(int weatherId)
        {
            string weather = " ";
            switch (weatherId)
            {
                case 1:
                    weather = Weather.Warmanddry.ToString();
                    break;
                case 2:
                    weather = Weather.ColdMountain.ToString();
                    break;
                case 3:
                    weather = Weather.MildAndHumid.ToString();
                    break;
                case 4:
                    weather = Weather.WarmAndHumid.ToString();
                    break;
            }

            return weather;
        }

    }
}
