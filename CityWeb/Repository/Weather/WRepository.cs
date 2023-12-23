using City_Web.Models.Data;
using CityWeb.Model.Data;
using CityWeb.Model.Interface;
using System.ComponentModel;
using WebApplication10.Models.Repository;

namespace CityWeb.Model.Repository
{
    public class WRepository : IWeatherRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICityRepository _cityRepository;
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
        public WRepository(ApplicationDbContext context, ICityRepository cityRepository)
        {
            this._context = context;
            this._cityRepository = cityRepository;
        }

        public WeatherData getWeatherData(int cityId)
        {
            var weatherData = _context.Weather.Where(c => c.CityID == cityId).FirstOrDefault();
            return weatherData;
        }

        public bool isUpdateWeather(string name)
        {
            var city = _cityRepository.getByCityName(name);
            bool flag;
            var nowTime = DateTime.Now;
            var timeUpdateWeather = city.TimeUpdateWeather;
            if ((timeUpdateWeather.Year - nowTime.Year) < 0)
                flag = true;
            else
            {
                if ((timeUpdateWeather.Month - nowTime.Month) < 0)
                    flag = true;
                else
                {
                    if ((timeUpdateWeather.Day - nowTime.Day) < 0)
                        flag = true;
                    else
                        flag = false;
                }
            }
            return flag;
        }

        public bool update(WeatherData weatherData)
        {

            _context.Update(weatherData);
            return save();
        }

        public bool add(WeatherData weather)
        {
            _context.Add(weather);
            return save();
        }

        public bool delete(WeatherData weatherData)
        {
            _context.Remove(weatherData);
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
