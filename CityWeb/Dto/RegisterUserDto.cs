using System.ComponentModel.DataAnnotations;

namespace CityWeb.Auth.Model
{
    public class RegisterUserDto
    {
        [Required()]
        public string UserName { get; set; }

        [Required()]
        public string Email { get; set; }

        [Required()]
        public string Password { get; set; }
        [Required()]
        public string cityName { get; set; }
        [Required()]
        public string countryName { get; set; }
        [Required()]
        public long PopulationCity { get; set; }
        [Required()]
        public int WeatherId { get; set; }

    }
}
