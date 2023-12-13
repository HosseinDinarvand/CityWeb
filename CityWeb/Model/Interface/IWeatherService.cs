namespace CityWeb.Model.Interface
{
    public interface IWeatherService
    {
        Task<List<CityWeb.Model.Weather.Weather>> GetWeatherDataAsync(string city);
    }
}
