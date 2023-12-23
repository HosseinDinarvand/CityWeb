using CityWeb.Auth;
using CityWeb.Model.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace City_Web.Models.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=10.51.10.79,1446;Initial Catalog=CityDb;User ID=sa;Password=Academy1234;TrustServerCertificate=True");

        }
        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);
        }
        public DbSet<City> city { get; set; }
        public DbSet<Country> country { get; set; }
        public DbSet<WeatherData> Weather { get; set; }
    }
}
