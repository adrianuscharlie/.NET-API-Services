using StackExchange.Redis;

namespace CashoutServices.Services
{
    public class RedisServices
    {

        private readonly IDatabase _redisDB;
        public RedisServices(IConnectionMultiplexer redis)
        {
            _redisDB=redis.GetDatabase();
        }


        public string? GetToken(string key)
        {
            return _redisDB.StringGet(key);
        }

        public void SetToken(string key,string token, TimeSpan expires)
        {
            _redisDB.StringSet(key,token,expires);
        }
        public void RemoveToken(string key)
        {
            _redisDB.KeyDelete(key);
        }
    }
}
