using CityWeb.Helper;
using CityWeb.Model.Interface;

using Newtonsoft.Json;
using StackExchange.Redis;
//using StackExchange.Redis;

namespace CityWeb.Model.Repository
{
    public class CacheSrvice : ICacheService
    {
        private IDatabase _db;
        public CacheSrvice()
        {
            ConfigureRedis();
        }
        private void ConfigureRedis()
        {
            _db = ConnectionHelper.Connection.GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value = _db.StringGet(key);
            if(!string.IsNullOrEmpty(value))
                return JsonConvert.DeserializeObject<T>(value);

            return default;
        }

        public object RemoveData(string key)
        {
            bool _isKeyExist = _db.KeyExists(key);
            if( _isKeyExist == true)
            {
                return _db.KeyDelete(key);
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationtime)
        {
            TimeSpan expiryTime = expirationtime.DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonConvert.SerializeObject(value), expiryTime);
            return isSet;
        }
    }
}
