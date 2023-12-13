using CityWeb.Model.Data;
using CityWeb.Model.Interface;
using CityWeb.Model.Weather;
using Newtonsoft.Json;

namespace CityWeb.Model.Repository
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            this._httpClient = httpClient;
            this._apiKey = configuration["OpenWeatherMapApiKey"];
        }

        public async Task<List<CityWeb.Model.Weather.Weather>> GetWeatherDataAsync(string city)
        {
            string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}";
            string response = await _httpClient.GetStringAsync(apiUrl);
            DataContainer data = JsonConvert.DeserializeObject<DataContainer>(response);
            return data.Weather;
        }
    }
}
