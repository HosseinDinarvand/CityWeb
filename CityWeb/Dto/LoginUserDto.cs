using System.ComponentModel.DataAnnotations;

namespace CityWeb.Auth.Model
{
    public class LoginUserDto
    {
        [Required(ErrorMessage = "User name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
