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
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CityController> _logger;
        private readonly IMapper _mapper;

        public CityController(
            ICityRepository cityRpository,
            ICountryRepository countryRepository,
            IWeatherRepository weatherRepository,
            ICacheService cacheService,
            IWeatherService weatherService,
            ApplicationDbContext context,
            ILogger<CityController> logger,
            IMapper mapper)
        {
            this._cityRpository = cityRpository;
            this._countryRpository = countryRepository;
            this._weatherRepository = weatherRepository;
            this._cacheService = cacheService;
            this._weatherService = weatherService;
            this._context = context;
            this._logger = logger;
            this._mapper = mapper;
        }

        private static object _lock = new object();
        [Authorize(Roles = "User")]
        [HttpGet("showAllCities")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<City>))]
        public async Task<IEnumerable<CityDto>> getCities()
        {
            List<City> _cities = _cityRpository.getCities();

            foreach (var city in _cities)
            {
                if (_weatherRepository.isUpdateWeather(city.Name))
                {
                    var _weatherData = await _weatherService.GetWeatherDataAsync(city.Name);
                    WeatherDto weatherData = new WeatherDto
                    {
                        description = _weatherData.FirstOrDefault().description,
                        icon = _weatherData.FirstOrDefault().icon,
                        main = _weatherData.FirstOrDefault().main,
                        CityID = city.Id
                    };

                    var weather = _weatherRepository.getWeatherData(city.Id);
                    if (weather != null)
                        _weatherRepository.delete(weather);

                    var _weather = _mapper.Map<WeatherData>(weatherData);

                    city.TimeUpdateWeather = DateTime.Now;
                    city.CurrentWeather = weatherData.description;

                    _weatherRepository.add(_weather);
                }
            }

            var cities = _mapper.Map<List<CityDto>>(_cities);
            return cities;
        }

        [Authorize(Roles = "User")]
        [HttpGet("cityName")]
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

            //int hourDifference = city.TimeUpdateWeather.Hour - DateTime.Now.Hour;
            if (_weatherRepository.isUpdateWeather(city.Name))
            {
                var _weatherData = await _weatherService.GetWeatherDataAsync(name);
                WeatherData weatherData = new WeatherData
                {
                    CityID = _cityRpository.getByCityName(name).Id,
                    description = _weatherData.FirstOrDefault().description,
                    icon = _weatherData.FirstOrDefault().icon,
                    main = _weatherData.FirstOrDefault().main,
                };

                var w = _weatherRepository.getWeatherData(_cityRpository.getByCityName(name).Id);
                if (w != null)
                    _weatherRepository.delete(w);

                _weatherRepository.add(weatherData);
                city.TimeUpdateWeather = DateTime.Now;
                city.CurrentWeather = weatherData.description;
            }

            cities.Add(city);
            _cacheService.SetData<IEnumerable<CityDto>>("city by name", cities, expirationTime);
            return Ok(city);

        }

        [Authorize(Roles = "User")]
        [HttpGet("countryName")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<City>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> getCityByCountryName(string countryName)
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

            foreach (var city in cities)
            {
                if (_weatherRepository.isUpdateWeather(city.Name))
                {
                    var _weatherData = await _weatherService.GetWeatherDataAsync(city.Name);

                    WeatherDto weatherDto = new WeatherDto
                    {
                        description = _weatherData.FirstOrDefault().description,
                        icon = _weatherData.FirstOrDefault().icon,
                        main = _weatherData.FirstOrDefault().main,
                        CityID = _cityRpository.getByCityName(city.Name).Id,
                    };

                    var weather = _weatherRepository.getWeatherData(_cityRpository.getByCityName(city.Name).Id);
                    if (weather != null)
                        _weatherRepository.delete(weather);

                    var _weather = _mapper.Map<WeatherData>(weatherDto);

                    city.TimeUpdateWeather = DateTime.Now;
                    city.CurrentWeather = weatherDto.description;

                    _weatherRepository.add(_weather);
                }

            }
            cacheData = cities.ToList();
            _cacheService.SetData<IEnumerable<CityDto>>("city by country name", cacheData, expirationTime);

            return Ok(cities);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addCity")]
        [ProducesResponseType(400)]
        public async Task<IActionResult> create(string name, long population, string countryName, int weatherId)
        {
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
                Name = name,
                Population = population,
                CountryName = countryName,
                WeatherId = weatherId,
            };

            var cityMap = _mapper.Map<City>(cityCreate);

            var weatherData = await _weatherService.GetWeatherDataAsync(name);

            WeatherData weather = new WeatherData
            {
                description = weatherData.FirstOrDefault().description,
                icon = weatherData.FirstOrDefault().icon,
                main = weatherData.FirstOrDefault().main,
            };

            try
            {
                cityMap.CurrentWeather = weatherData.FirstOrDefault().description;
                cityMap.TimeUpdateWeather = DateTime.Now;
                cityMap.countryId = country.Id;
                cityMap.Weather = _weatherRepository.setWeather(cityCreate.WeatherId);
                cityMap.CurrentWeather = weather.description;
                _cityRpository.add(cityMap);
                weather.CityID = _cityRpository.getByCityName(name).Id;
                _weatherRepository.add(weather);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Ok("Successfully created");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("updateCity")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCity(string name,string countryName,long population,int weatherId)
        {
            var city = _cityRpository.getByCityNameNoTracking(name);

            var country = _countryRpository.getByCountryName(countryName);

            if (!_cityRpository.existCity(name)) return NotFound();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var UpdateCity = new CityDto
            {
                Name = name,
                Population = population,
                CountryName = countryName,
                WeatherId = weatherId
            };

            var cityMap = _mapper.Map<City>(UpdateCity);
            cityMap.Id = city.Id;
            cityMap.Weather = _weatherRepository.setWeather(weatherId);
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
        [HttpDelete("deleteCity")]
        [ProducesResponseType(404)]
        public IActionResult DeleteCity(string cityName)
        {
            if (!_cityRpository.existCity(cityName)) return NotFound();

            var weather = _weatherRepository.getWeatherData(_cityRpository.getByCityName(cityName).Id);

            var city = _cityRpository.getByCityName(cityName);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var users = _context.Users.Where(c => c.CityId == city.Id);
            foreach (var user in users)
            {
                user.CityId = null;
            }

            _cityRpository.delete(city);
            _weatherRepository.delete(weather);

            _cacheService.RemoveData("city by country name");
            _cacheService.RemoveData("city by name");

            return Ok("Successfuly delete");
        }
    }
}
