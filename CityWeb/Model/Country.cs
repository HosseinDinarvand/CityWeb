using CityWeb.Auth;
using System.ComponentModel.DataAnnotations;

namespace City_Web.Models.Data
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Country name is required")]
        public string Name { get; set; } = String.Empty;
        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
