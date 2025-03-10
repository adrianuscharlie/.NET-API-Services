using StackExchange.Redis;

namespace CashoutServices.Services
{
    public class RedisServices
    {

        
        private static readonly Lazy<ConnectionMultiplexer> _lazyConnection =
        new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("localhost:6379"));

        private static ConnectionMultiplexer Connection => _lazyConnection.Value;
        private readonly IDatabase _redisDB = Connection.GetDatabase();
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
