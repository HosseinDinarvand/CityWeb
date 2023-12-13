using System.ComponentModel.DataAnnotations;

namespace CityWeb.Dto
{
    public class CountryDto
    {
        [Required(ErrorMessage = "Country name is required")]
        public string Name { get; set; }
    }
}
