using AutoMapper;
using City_Web.Models.Data;
using CityWeb.Dto;
using CityWeb.Model.Data;
using CityWeb.Model.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication10.Models.Interface;
using WebApplication10.Models.Repository;
//using WebApplication10.ViewModels;

namespace WebApplication10.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ICityRepository _cityRpository;
        private readonly ICountryRepository _countryRpository;
        private readonly IWeatherRepository _weatherRepository;
        private readonly IWeatherService _weatherService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CityController> _logger;
        private readonly IMapper _mapper;

        public CityController(
            ICityRepository cityRpository,
            ICountryRepository countryRepository,
            IWeatherRepository weatherRepository,
            ICacheService cacheService,
            IWeatherService weatherService,
            ILogger<CityController> logger,
            IMapper mapper)
        {
            this._cityRpository = cityRpository;
            this._countryRpository = countryRepository;
            this._weatherRepository = weatherRepository;
            this._cacheService = cacheService;
            this._weatherService = weatherService;
            this._logger = logger;
            this._mapper = mapper;
        }

        private static object _lock = new object();
        [Authorize(Roles = "User")]
        [HttpGet("Show all cities")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<City>))]
        public IEnumerable<CityDto> getCities()
        {
            List<City> _cities = _cityRpository.getCities();
            var cities = _mapper.Map<List<CityDto>>(_cities);
            return cities;
        }

        [Authorize(Roles = "User")]
        [HttpGet("City Name")]
        [ProducesResponseType(200, Type = typeof(City))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> getCityByName(string name)
        {
            List<CityDto> cities = new List<CityDto>();
            List<CityDto> cityCache = new List<CityDto>();

            var cacheData = _cacheService.GetData<IEnumerable<CityDto>>("city by name");
            if (cacheData != null)
            {
                cityCache = cacheData.Where(c => c.Name == name).ToList();
                if (cityCache.Count > 0)
                    return Ok(cityCache);
            }

            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);

            var city = _mapper.Map<CityDto>(_cityRpository.getByCityName(name));

            int hourDifference = city.TimeUpdateWeather.Hour - DateTime.Now.Hour;
            int secondDifference = city.TimeUpdateWeather.Second - DateTime.Now.Second;
            if (hourDifference > 24 && secondDifference > 0)
            {
                var _weatherData = await _weatherService.GetWeatherDataAsync(name);
                WeatherData weatherData = new WeatherData
                {
                    CityID = _cityRpository.getByCityName(name).Id,
                    description = _weatherData.FirstOrDefault().description,
                    icon = _weatherData.FirstOrDefault().icon,
                    main = _weatherData.FirstOrDefault().main,
                };
                city.TimeUpdateWeather = DateTime.Now;
                city.CurrentWeather = _weatherData.FirstOrDefault().description;
            }

            cities.Add(city);
            _cacheService.SetData<IEnumerable<CityDto>>("city by name", cities, expirationTime);
            return Ok(city);

        }

        [Authorize(Roles = "User")]
        [HttpGet("Country Name")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<City>))]
        [ProducesResponseType(400)]
        public IActionResult getCityByCountryName(string countryName)
        {
            var cacheData = _cacheService.GetData<IEnumerable<CityDto>>("city by country name");
            if (cacheData != null)
            {
                cacheData = cacheData.Where(c => c.CountryName == countryName).ToList();
                return Ok(cacheData);
            }


            if (_cityRpository.getCityByContryName(countryName) == null)
                return null;

            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            var cities = _mapper.Map<IEnumerable<CityDto>>(_cityRpository.getCityByContryName(countryName));
            cacheData = cities.ToList();
            _cacheService.SetData<IEnumerable<CityDto>>("city by country name", cacheData, expirationTime);

            return Ok(cities);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add city")]
        [ProducesResponseType(400)]
        public async Task<IActionResult> create(string name,long population,string countryName,int weatherId)
        {
            var weatherData = await _weatherService.GetWeatherDataAsync(name);
            var c = _cityRpository.getByCityName(name);
            if (c != null)
            {
                ModelState.AddModelError("", "This City already exist");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var country = _countryRpository.getByCountryName(countryName);
            if (country == null)
            {
                var newCountry = new CountryDto
                {
                    Name = countryName,
                };

                var countryMap = _mapper.Map<Country>(newCountry);

                _countryRpository.add(countryMap);
                country = _countryRpository.getByCountryName(countryName);
            }

            CityDto cityCreate = new CityDto
            {
                Name=name,
                Population=population,
                CountryName=countryName,
                WeatherId=weatherId,
            };

            try
            {
                var cityMap = _mapper.Map<City>(cityCreate);
                cityMap.CurrentWeather = weatherData.FirstOrDefault().description;
                cityMap.TimeUpdateWeather = DateTime.Now;
                cityMap.countryId = country.Id;
                cityMap.Weather = _weatherRepository.setWeather(cityCreate.WeatherId);
                _cityRpository.add(cityMap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Ok("Successfully created");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCity([FromBody] CityDto updateCity)
        {
            var city = _cityRpository.getByCityNameNoTracking(updateCity.Name);
            var country = _countryRpository.getByCountryName(updateCity.CountryName);

            if (updateCity == null) return BadRequest(ModelState);

            if (updateCity.Name != city.Name) return BadRequest(ModelState);

            if (!_cityRpository.existCity(updateCity.Name)) return NotFound();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var UpdateCity = new CityDto
            {
                Name = updateCity.Name,
                Population = updateCity.Population,
                CountryName = updateCity.CountryName,
                WeatherId = updateCity.WeatherId
            };

            var cityMap = _mapper.Map<City>(UpdateCity);
            cityMap.Id = city.Id;
            cityMap.Weather = _weatherRepository.setWeather(updateCity.WeatherId);
            cityMap.countryId = country.Id;

            if (!_cityRpository.update(cityMap))
            {
                _logger.LogError("city does not update successfuly.");
                ModelState.AddModelError("", "Something went wrong updating city");
                return StatusCode(500, ModelState);
            }

            _cacheService.RemoveData("city by country name");
            _cacheService.RemoveData("city by name");

            _cityRpository.update(cityMap);
            return Ok("Successfuly update");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        [ProducesResponseType(404)]
        public IActionResult DeleteCity(string cityName)
        {
            if (!_cityRpository.existCity(cityName)) return NotFound();

            var city = _cityRpository.getByCityNameNoTracking(cityName);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_cityRpository.delete(city))
            {
                _logger.LogError("city does not delete.");
                ModelState.AddModelError("", "Something went wrong deleting city");
            }

            _cityRpository.delete(city);

            _cacheService.RemoveData("city by country name");
            _cacheService.RemoveData("city by name");

            return Ok("Successfuly delete");
        }
    }
}
