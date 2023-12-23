using AutoMapper;
using City_Web.Models.Data;
using CityWeb.Auth;
using CityWeb.Auth.Model;
using CityWeb.Dto;
using CityWeb.Model.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication10.Models.Interface;
using WebApplication10.Models.Repository;

namespace CityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IWeatherRepository _weatherRepository;
        private readonly IJwtManagerRepository _jwtManagerRepository;
        private readonly ILogger<AuthenticateController> _logger;
        private readonly IMapper _mapper;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ICityRepository cityRepository,
            ICountryRepository countryRepository,
            IWeatherRepository weatherRepository,
            IJwtManagerRepository jwtManagerRepository,
            ILogger<AuthenticateController> logger,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
            _weatherRepository = weatherRepository;
            _jwtManagerRepository = jwtManagerRepository;
            _logger = logger;
            _mapper = mapper;
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                try
                {
                    _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
                    var token = _jwtManagerRepository.CreateToken(authClaims);
                    var refreshToken = _jwtManagerRepository.GenerateRefreshToken();
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("CreateToken and GenerateRefreshToken methods called successfuly.");
                    return Ok(new
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        RefreshToken = refreshToken,
                        Expiration = token.ValidTo
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            return Unauthorized();
        }


        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var userExist = await _userManager.FindByNameAsync(model.UserName);
            if (userExist != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            CityDto cityDto = new CityDto
            {
                Name = model.cityName,
                CountryName = model.countryName,
                Population = model.PopulationCity,
                WeatherId = model.WeatherId,
            };
            CountryDto countryDto = new CountryDto
            {
                Name = model.countryName
            };

            var country = _countryRepository.getByCountryName(model.countryName);

            var city = _cityRepository.getByCityName(model.cityName);

            if (country == null)
            {
                var countryMap = _mapper.Map<Country>(countryDto);
                _countryRepository.add(countryMap);
                country = _countryRepository.getByCountryName(model.countryName);
            }

            if (city == null)
            {
                var cityMap = _mapper.Map<City>(cityDto);
                cityMap.Weather = _weatherRepository.setWeather(model.WeatherId);
                _cityRepository.add(cityMap);
                city = _cityRepository.getByCityName(model.cityName);
            }

            ApplicationUser user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                CityName = model.cityName,
                countryName = model.countryName,
                CountryId = country.Id,
                CityId = city.Id,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("user not created.");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }
            _logger.LogInformation("user successfuly created.");
         
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _userManager.AddToRoleAsync(user, UserRoles.User);

            return Ok(new Response { Status = "Success", Message = "User created cuccessfuly!" });
        }


        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin( RegisterUserDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            var city = _cityRepository.getByCityName(model.cityName);

            var country = _countryRepository.getByCountryName(model.countryName);

            CityDto cityDto = new CityDto
            {
                Name = model.cityName,
                CountryName = model.countryName,
                Population = model.PopulationCity,
                WeatherId = model.WeatherId,
            };
            CountryDto countryDto = new CountryDto
            {
                Name = model.countryName
            };

            if (country == null)
            {
                var countryMap = _mapper.Map<Country>(countryDto);
                _countryRepository.add(countryMap);
                country = _countryRepository.getByCountryName(model.countryName);
            }

            if (city == null)
            {
                var cityMap = _mapper.Map<City>(cityDto);
                cityMap.Weather = _weatherRepository.setWeather(model.WeatherId);
                _cityRepository.add(cityMap);
                city = _cityRepository.getByCityName(model.cityName);
            }

            ApplicationUser user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                CityName = model.cityName,
                countryName = model.countryName,
                CityId = city.Id,
                CountryId = country.Id,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Admin not successfuly created.");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            _logger.LogInformation("Admin successfuly created.");

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);



            return Ok(new Response { Status = "Success", Message = "Admin created successfully!" });
        }


        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(Token tokenModel)
        {
            if (tokenModel is null)
                return BadRequest("Invalid client request");

            string? accessToken = tokenModel.AccessToken;
            string? refreshToekn = tokenModel.RefreshToken;

            var principal = _jwtManagerRepository.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                _logger.LogError("token model is not valid; check it.");
                return BadRequest("Invalid access token or refresh token");
            }

            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToekn || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = _jwtManagerRepository.CreateToken(principal.Claims.ToList());
            var newRefreshToken = _jwtManagerRepository.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }


        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Invalid user name");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }


        [HttpPost]
        [Route("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
            }

            return NoContent();
        }

    }
}
