using Microsoft.AspNetCore.Identity;

namespace CityWeb.Auth
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string CityName { get; set; }
        public string countryName { get; set; }
        public int? CityId { get; set; }
        public int? CountryId { get; set; }
    }
}
