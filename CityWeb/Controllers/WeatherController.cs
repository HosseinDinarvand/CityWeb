using CityWeb.Model.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherService _weatherService;
        public WeatherController(WeatherService weatherService)
        {
            this._weatherService = weatherService;
        }


        [HttpGet("cityname")]
        public async Task<IActionResult> GetWeather(string cityName)
        {
            try
            {
                var weatherData = await _weatherService.GetWeatherDataAsync(cityName);
                return Ok(weatherData);
            }
            catch (HttpRequestException)
            {
                return BadRequest("Faild to retrieve weather data.");
            }
        }
    }
}
