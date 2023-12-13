using CityWeb.Model.Data;
namespace CityWeb.Model.Interface
{
    public interface IWeatherRepository
    {
        string setWeather(int weatherId);
        bool add(WeatherData weather);
        bool save();
    }
}
