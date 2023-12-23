using CityWeb.Model.Data;
namespace CityWeb.Model.Interface
{
    public interface IWeatherRepository
    {
        string setWeather(int weatherId);
        WeatherData getWeatherData(int cityId);
        bool isUpdateWeather(string name);
        bool update(WeatherData weatherData);
        bool delete(WeatherData weatherData);
        bool add(WeatherData weather);
        bool save();
    }
}
