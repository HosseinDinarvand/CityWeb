namespace CityWeb.Configuration
{
    static class CinfigurationManager
    {
        public static IConfiguration AppSetting { get; }

        static CinfigurationManager()
        {
            AppSetting = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        }
    }
}
